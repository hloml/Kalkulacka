var TariffLenght;
var TariffPrice;

var AverageFuelConsumption;
var LiterOfFuelPrice;
var PathDistance;
var ParkingFee;

var CarPriceOnKM;
var TariffPriceOnKM;

// Slider - fuelConsumption
$(function () {
    $("#fuelConsumption").slider({
        value: 8.5,
        min: 1,
        max: 20,
        step: 0.1,
        slide: function (event, ui) {
            $("#fuelConsumptionAmount").val(ui.value + " litr/100km");
            AverageFuelConsumption = ui.value;
            countAll();
        }
    });
    $("#fuelConsumptionAmount").val($("#fuelConsumption").slider("value") + " litr/100km");
    AverageFuelConsumption = $("#fuelConsumption").slider("value");
});
// Slider - fuelPrice
$(function () {
    $("#fuelPrice").slider({
        value: 35,
        min: 10,
        max: 50,
        step: 0.1,
        slide: function (event, ui) {
            $("#fuelPriceAmount").val(ui.value + " Kč/litr");
            LiterOfFuelPrice = ui.value;
            countAll();
        }
    });
    $("#fuelPriceAmount").val($("#fuelPrice").slider("value") + " Kč/litr");
    LiterOfFuelPrice = $("#fuelPrice").slider("value");
});
// Slider - pathDistance
$(function () {
    $("#pathDistance").slider({
        value: 10,
        min: 1,
        max: 80,
        step: 1,
        slide: function (event, ui) {
            $("#pathDistanceAmount").val(ui.value + " Km");
            PathDistance = ui.value;
            countAll();
        }
    });
    $("#pathDistanceAmount").val($("#pathDistance").slider("value") + " Km");
    PathDistance = $("#pathDistance").slider("value");
});
// Slider - parkingPrice
$(function () {
    $("#parkingPrice").slider({
        value: 0,
        min: 0,
        max: 300,
        step: 5,
        slide: function (event, ui) {
            $("#parkingPriceAmount").val(ui.value + " Kč/den");
            ParkingFee = ui.value;
            countAll();
        }
    });
    $("#parkingPriceAmount").val($("#parkingPrice").slider("value") + " Kč/den");
    ParkingFee = $("#parkingPrice").slider("value");
});


// --------------------------------- Core Code

function countAll() {
    // Help vars
    CarPriceOnKM = AverageFuelConsumption * LiterOfFuelPrice / 100;
    TariffPriceOnKM = TariffPrice / TariffLenght / PathDistance;
    // Counting functions
    function PriceOfPath() {
        return CarPriceOnKM * PathDistance + ParkingFee;
    }
    function KilometerPriceDiff() {
        return CarPriceOnKM - TariffPriceOnKM;
    }
    function OnDistancePriceDiff() {
        return KilometerPriceDiff() * PathDistance + ParkingFee;
    }
    function OnPrepaidTimePriceDiff() {
        return OnDistancePriceDiff() * TariffLenght;
    }
    function PercentPriceDiff() {
        if (KilometerPriceDiff() == 0) {
            return 0;
        }
        return 100 - (100 / (PriceOfPath() / (TariffPrice / TariffLenght)));
    }

    // Results out
    $("#ResultPriceOfPath").val(parseFloat(PriceOfPath()).toFixed(2) + " Kč");
    $("#ResultKilometerPriceDiff").val(parseFloat(KilometerPriceDiff()).toFixed(2) + " Kč");
    $("#ResultOnDistancePriceDiff").val(parseFloat(OnDistancePriceDiff()).toFixed(2) + " Kč");
    $("#ResultOnPrepaidTimePriceDiff").val(parseFloat(OnPrepaidTimePriceDiff()).toFixed(2) + " Kč");
    $("#ResultPercentPriceDiff").val(parseFloat(PercentPriceDiff()).toFixed(2) + " %");

    // Change color and help-block text
    if (PercentPriceDiff() < 0) {
        $(".has-success").removeClass("has-success").addClass("has-error");
        //$(".help-block").text("- prodělávám");
        $("#saving").removeClass("label-success").addClass("label-default");
        $("#loosing").removeClass("label-default").addClass("label-danger");
    } else {
        $(".has-error").removeClass("has-error").addClass("has-success");
        //$(".help-block").text("+ šetřím");
        $("#saving").removeClass("label-default").addClass("label-success");
        $("#loosing").removeClass("label-danger").addClass("label-default");
    }
}


// Inicialization
function compareWithCarInit(tariffLenght, tariffPrice) {
    TariffLenght = tariffLenght;
    TariffPrice = tariffPrice;
    countAll();
}