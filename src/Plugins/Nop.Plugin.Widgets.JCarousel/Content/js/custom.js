$(document).ready(function() {
    $('.jcarousel-list').owlCarousel({
        loop: true,
        margin: 10,
        responsiveClass: true,
        responsive: {
            0: {
                items: 2,
                dots: true,
                nav: true,
            },
            600: {
                items: 3,
                nav: true
            },
            1000: {
                items: 4,
                nav: true,
                loop: false
          },
            1300: {
            items: 5,
            nav: true,
            loop: false
          }
        }
    })

});