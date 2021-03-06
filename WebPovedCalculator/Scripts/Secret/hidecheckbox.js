﻿
$(function () {
    $("#category").change(function () {
        var str = "";
        $("#category option:selected").each(function () {
            str += $(this).text();
        });
        hideCheckbox(str);
    })
  .change();
});

function hideCheckbox(discount) {
    switch (discount) {
        case "Student (15 - 26 let)":
            $("#isicDiscount").show();
            $("#schoolDiscount").show();
            $("#janskehoDiscount").hide();
            $("#discountsJanskeho").prop('checked', false);
            break;
        case "ZTP":
            $("#isicDiscount").hide();
            $("#schoolDiscount").hide();
            $("#janskehoDiscount").hide();
            $("#discountISIC").prop('checked', false);
            $("#discountsSchool").prop('checked', false);
            $("#discountsJanskeho").prop('checked', false);
            break;
        case "Firemní":
            $("#isicDiscount").hide();
            $("#schoolDiscount").hide();
            $("#janskehoDiscount").hide();
            $("#discountISIC").prop('checked', false);
            $("#discountsSchool").prop('checked', false);
            $("#discountsJanskeho").prop('checked', false);
            break;
        case "Dítě (do 6 let)":
            $("#isicDiscount").hide();
            $("#schoolDiscount").hide();
            $("#janskehoDiscount").hide();
            $("#discountISIC").prop('checked', false);
            $("#discountsSchool").prop('checked', false);
            $("#discountsJanskeho").prop('checked', false);
            break;
        case "Důchodce (do 65 let)":
            $("#isicDiscount").hide();
            $("#schoolDiscount").hide();
            $("#janskehoDiscount").show();
            $("#discountISIC").prop('checked', false);
            $("#discountsSchool").prop('checked', false);
            break;
        case "Důchodce (65 - 70 let)":
            $("#isicDiscount").hide();
            $("#schoolDiscount").hide();
            $("#janskehoDiscount").show();
            $("#discountISIC").prop('checked', false);
            $("#discountsSchool").prop('checked', false);
            break;
        case "Důchodce (70 a více let)":
            $("#isicDiscount").hide();
            $("#schoolDiscount").hide();
            $("#janskehoDiscount").show();
            $("#discountISIC").prop('checked', false);
            $("#discountsSchool").prop('checked', false);
            break;
        case "Dítě (6 - 15 let)":
            $("#isicDiscount").hide();
            $("#schoolDiscount").show();
            $("#janskehoDiscount").hide();
            $("#discountISIC").prop('checked', false);
            $("#discountsJanskeho").prop('checked', false);
            break;
        case "Dospělý (od 15 let)":
            $("#isicDiscount").hide();
            $("#schoolDiscount").hide();
            $("#janskehoDiscount").show();
            $("#discountISIC").prop('checked', false);
            $("#discountsSchool").prop('checked', false);
            break;
        default:
            break;

    }
}