// Used for compare with car part
var TariffLenght;
var TariffPrice;

var AverageFuelConsumption;
var LiterOfFuelPrice;
var PathDistance;
var ParkingFee;

var CarPriceOnKM;
var TariffPriceOnKM;

var rgx = /(\d+)(\d{3})/;

var koef;

// Days in week car
$(function () {
    $("#compare_mon").change(function () {
        countDaysInWeek();
    })
  .change();
    $("#compare_tue").change(function () {
        countDaysInWeek();
    })
  .change();
    $("#compare_wed").change(function () {
        countDaysInWeek();
    })
  .change();
    $("#compare_thu").change(function () {
        countDaysInWeek();
    })
  .change();
    $("#compare_fri").change(function () {
        countDaysInWeek();
    })
  .change();
    $("#compare_sat").change(function () {
        countDaysInWeek();
    })
  .change();
    $("#compare_sun").change(function () {
        countDaysInWeek();
    })
  .change();
});

// Slider - fuelConsumption
function sliderConsumption(def) {
    $("#fuelConsumption").slider({
        value: def,
        min: 1,
        max: 20,
        step: 0.1,
        slide: function (event, ui) {
            $("#fuelConsumptionAmount").val(ui.value + " litr/100km");
            $("#compare_averageFuelConsumption").val(ui.value.toString().replace(/\./g, ','));
            AverageFuelConsumption = ui.value;
            countAll();
        }
    });
    $("#fuelConsumptionAmount").val($("#fuelConsumption").slider("value") + " litr/100km");
    $("#compare_averageFuelConsumption").val($("#fuelConsumption").slider("value").toString().replace(/\./g, ','));
    AverageFuelConsumption = $("#fuelConsumption").slider("value");
}
// Slider - fuelPrice
function sliderPrice(def) {
    $("#fuelPrice").slider({
        value: def,
        min: 10,
        max: 50,
        step: 0.1,
        slide: function (event, ui) {
            $("#fuelPriceAmount").val(ui.value + " Kč/litr");
            $("#compare_literOfFuelPrice").val(ui.value.toString().replace(/\./g, ','));
            LiterOfFuelPrice = ui.value;
            countAll();
        }
    });
    $("#fuelPriceAmount").val($("#fuelPrice").slider("value") + " Kč/litr");
    $("#compare_literOfFuelPrice").val($("#fuelPrice").slider("value").toString().replace(/\./g, ','));
    LiterOfFuelPrice = $("#fuelPrice").slider("value");
}
// Slider - pathDistance
function sliderDistance(def) {
    $("#pathDistance").slider({
        value: def,
        min: 1,
        max: 80,
        step: 1,
        slide: function (event, ui) {
            $("#pathDistanceAmount").val(ui.value + " Km");
            $("#compare_pathDistance").val(ui.value.toString().replace(/\./g, ','));
            PathDistance = ui.value;
            countAll();
        }
    });
    $("#pathDistanceAmount").val($("#pathDistance").slider("value") + " Km");
    $("#compare_pathDistance").val($("#pathDistance").slider("value").toString().replace(/\./g, ','));
    PathDistance = $("#pathDistance").slider("value");
}
// Slider - parkingPrice
function sliderParking(def) {
    $("#parkingPrice").slider({
        value: def,
        min: 0,
        max: 300,
        step: 5,
        slide: function (event, ui) {
            $("#parkingPriceAmount").val(ui.value + " Kč/den");
            $("#compare_parkingFee").val(ui.value.toString().replace(/\./g, ','));
            ParkingFee = ui.value;
            countAll();
        }
    });
    $("#parkingPriceAmount").val($("#parkingPrice").slider("value") + " Kč/den");
    $("#compare_parkingFee").val($("#parkingPrice").slider("value").toString().replace(/\./g, ','));
    ParkingFee = $("#parkingPrice").slider("value");
}


// --------------------------------- Core Code

// Counting function for comparing public transport tariff with car, writting results into page
function countAll() {
    // Help vars
    CarPriceOnKM = AverageFuelConsumption * LiterOfFuelPrice / 100;
    TariffPriceOnKM = TariffPrice / TariffLenght / PathDistance;
    // Counting functions
    function PriceOfPath() {
        return CarPriceOnKM * PathDistance + ParkingFee;
    }
    function KilometerPriceDiff() {
        return (CarPriceOnKM * koef - TariffPriceOnKM);
    }
    function OnDistancePriceDiff() {
        return KilometerPriceDiff() * PathDistance + (ParkingFee * koef);
    }
    function OnPrepaidTimePriceDiff() {
        return OnDistancePriceDiff() * TariffLenght;
    }
    function PercentPriceDiff() {
        if (KilometerPriceDiff() == 0) {
            return 0;
        }
        return 100 - (100 / ((PriceOfPath() * koef) / (TariffPrice / TariffLenght)));
    }

    // Results out
    $("#ResultPriceOfPath").text(parseFloat(PriceOfPath()).toFixed(2) + " Kč");
    $("#ResultKilometerPriceDiff").text(parseFloat(KilometerPriceDiff()).toFixed(2) + " Kč");
    $("#ResultOnDistancePriceDiff").text(parseFloat(OnDistancePriceDiff()).toFixed(2) + " Kč");
    $("#ResultOnPrepaidTimePriceDiff").text(parseFloat(OnPrepaidTimePriceDiff()).toFixed(2).replace(rgx, '$1' + ' ' + '$2') + " Kč");
    //$("#ResultPercentPriceDiff").text(parseFloat(PercentPriceDiff()).toFixed(2) + " %");

    // Change color and help-block text and smile
    if (PercentPriceDiff() < 0) {
        //$(".help-block").text("- prodělávám");
        $("#saving").removeClass("label-success").addClass("label-default");
        $("#loosing").removeClass("label-default").addClass("label-danger");
        $(".text-success").removeClass("text-success").addClass("text-danger");
        $(".saving-loosing").text("Prodělávám").removeClass("label-success").addClass("label-danger");
        $("#smile").text("");
    } else {
        //$(".help-block").text("+ šetřím");
        $("#saving").removeClass("label-default").addClass("label-success");
        $("#loosing").removeClass("label-danger").addClass("label-default");
        $(".text-danger").removeClass("text-danger").addClass("text-success");
        $(".saving-loosing").text("Šetřím").removeClass("label-danger").addClass("label-success");
        $("#smile").html("<img src=\"/Content/smile/smiley-dance.gif\" alt=\"šetřím\" \\>");
    }
}

// counts coeficient of days in week for multiply
function countDaysInWeek() {
    var k = 0;
    if ($("#compare_mon").is(":checked")) {
        k = k + 1;
    }
    if ($("#compare_tue").is(":checked")) {
        k = k + 1;
    }
    if ($("#compare_wed").is(":checked")) {
        k = k + 1;
    }
    if ($("#compare_thu").is(":checked")) {
        k = k + 1;
    }
    if ($("#compare_fri").is(":checked")) {
        k = k + 1;
    }
    if ($("#compare_sat").is(":checked")) {
        k = k + 1;
    }
    if ($("#compare_sun").is(":checked")) {
        k = k + 1;
    }
    koef = k/7;
    countAll();
}


// Inicialization, set counted tariff's length and price
function compareWithCarInit(
    tariffLenght, tariffPrice, // tariff
    consumption, price, distance, parking // car´params
    ) {
    TariffLenght = tariffLenght;
    TariffPrice = tariffPrice;

    sliderConsumption(consumption);
    sliderPrice(price);
    sliderDistance(distance);
    sliderParking(parking);

    countDaysInWeek();
}