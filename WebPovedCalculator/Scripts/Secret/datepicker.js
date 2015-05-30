// Used for datepickers

// CS locale
$.datepicker.regional['cs'] = {
    closeText: 'Zavřít',
    prevText: '&#x3c;Dříve',
    nextText: 'Později&#x3e;',
    currentText: 'Nyní',
    monthNames: ['leden', 'únor', 'březen', 'duben', 'květen', 'červen', 'červenec', 'srpen',
        'září', 'říjen', 'listopad', 'prosinec'],
    monthNamesShort: ['leden', 'únor', 'březen', 'duben', 'květen', 'červen', 'červenec', 'srpen',
        'září', 'říjen', 'listopad', 'prosinec'],
    //monthNamesShort: ['led', 'úno', 'bře', 'dub', 'kvě', 'čer', 'čvc', 'srp', 'zář', 'říj', 'lis', 'pro'],
    dayNames: ['neděle', 'pondělí', 'úterý', 'středa', 'čtvrtek', 'pátek', 'sobota'],
    dayNamesShort: ['ne', 'po', 'út', 'st', 'čt', 'pá', 'so'],
    dayNamesMin: ['ne', 'po', 'út', 'st', 'čt', 'pá', 'so'],
    weekHeader: 'Týd',
    dateFormat: 'dd/mm/yy',
    firstDay: 1,
    isRTL: false,
    showMonthAfterYear: false,
    yearSuffix: ''
};
$.datepicker.setDefaults($.datepicker.regional['cs']);

// Inicialization of calendars, set default start date and end date
function calendarInit(startDate, endDate) {
    $("#startDate").datepicker({
        changeMonth: true,
        changeYear: true,
        dateFormat: "d.m.yy",
        yearRange: startDate + ':' + endDate,
        onClose: function (selectedDate) {
            $("#endDate").datepicker("option", "minDate", selectedDate);
        }
    });
    $("#endDate").datepicker({
        changeMonth: true,
        changeYear: true,
        dateFormat: "d.m.yy",
        yearRange: startDate + ":" + endDate,
        onClose: function (selectedDate) {
            $("#startDate").datepicker("option", "maxDate", selectedDate);
        }
    });
}