﻿$(function () {
    $(window).on('scroll', () => {
        if ($(this).scrollTop() > 50) {
            $('.navbar').addClass('bg-dark');
        }
        else {
            $('.navbar').removeClass('bg-dark');
        }
    })
}); 
