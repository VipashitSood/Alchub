$(document).ready(function () {
  $('.lazy').lazy({
    combined: true,
    delay: 4000
  });
});

$(function () {
  window.onscroll = function () { myFunction() };

  var header = document.getElementById("myHeader");
  var sticky = header.offsetTop;

  function myFunction() {
    if (window.pageYOffset > sticky) {
      header.classList.add("sticky");
    } else {
      header.classList.remove("sticky");
    }
  }
  // mobile header//
  $(document).on("click", ".sidebar-button", function () {
    $("body").addClass("open-mobilemenu");
    $(".header").addClass("bg-color");
  });

  $(document).on("click", ".Mobileheader__closeMenu", function () {
    $("body").removeClass("open-mobilemenu");
  });



  if ($(window).width() < 992) {
    $(".Mobileheader__mobilesearch").hide();
    $(document).on("click", ".search-mobile", function () {
      $(".Mobileheader__mobilesearch").slideToggle();
      $(".Mobileheader__mobilesearch").show();
      $(".ico-wrap .header-links").hide();
      $(".search-box-text").removeAttr('placeholder');
      $(".search-box-text ").toggleClass("input-focus");
      $(".input-focus ").focus();
    });

    $(".Mobileheader__mobilesearch").append($(".store-search-box"));

    $(window).scroll(function () {
      if ($(window).scrollTop() > 50) {
        $(".input-focus ").blur();

      } else {
        $(".input-focus ").focus();
      }
    });

  }



  $(".textbox").focus(function () {
    ($(this).parent().addClass("changelabel"));

  }).blur(function () {
    $(this).parent().removeClass("changelabel");
  });

  $(function () {
    var loc = window.location.href; // returns the full URL

    var elements = document.getElementsByClassName("active-step");
    const n = elements.length;
    //console.log(elements[n - 1]);
    $(elements[n - 1]).addClass('last');

    if (elements[n]) {
      $('.order-progress ul .active-step').addClass('last');
    }

    else {
      $('order-progress ul li').removeClass('.last');
    }
  });

  $(document).on('click', '.open-comment', function (e) {
    e.preventDefault();
    $('.enter-address ').slideDown();
    $('.btn-holder__commentButton').addClass('active-comment');
  });

  $(document).on('click', '.close-comment', function (e) {
    e.preventDefault();
    $('.enter-address ').slideUp();
    $('.btn-holder__commentButton').removeClass('active-comment');
  });

  $('.open-comment').click(function () {
    $('.btn-holder').addClass("active");
  });

  $('.close-comment').click(function () {
    $('.btn-holder').removeClass("active");
  });

});

$(".open-comment").click(function () {
  $(".leaveComment").slideToggle('slow')
})

$("order-progress").on("click", function () {

  $('active-step').addClass("last").siblings().removeClass('last');

})
$('.order-progress ul .active-step ').on(function () {
  $('order-progress ul li').addClass(".last").siblings().removeClass('.last');
  $(this).addClass("last").siblings().removeClass('last');

});


$(function () {
  var loc = window.location.href; // returns the full URL

  var elements = document.getElementsByClassName("active-step");
  const n = elements.length;
  //console.log(elements[n - 1]);
  $(elements[n - 1]).addClass('last');

  if (elements[n]) {
    $('.order-progress ul .active-step').addClass('last');
  }

  else {
    $('order-progress ul li').removeClass('.last');
  }
});

function getParameterByName(name, url) {
  if (!url) url = window.location.href;
  name = name.replace(/[\[\]]/g, '\\$&');
  var regex = new RegExp('[?&]' + name + '(=([^&#]*)|&|#|$)'),
    results = regex.exec(url);
  if (!results) return null;
  if (!results[2]) return '';
  return decodeURIComponent(results[2].replace(/\+/g, ' '));
}

function updateQueryStringParameter(uri, key, value) {
  var re = new RegExp("([?&])" + key + "=.*?(&|$)", "i");
  var separator = uri.indexOf('?') !== -1 ? "&" : "?";
  if (uri.match(re)) {
    return uri.replace(re, '$1' + key + "=" + value + '$2');
  }
  else {
    return uri + separator + key + "=" + value;
  }
}

function removeParam(key, sourceURL) {
  var rtn = sourceURL.split("?")[0],
    param,
    params_arr = [],
    queryString = (sourceURL.indexOf("?") !== -1) ? sourceURL.split("?")[1] : "";
  if (queryString !== "") {
    params_arr = queryString.split("&");
    for (var i = params_arr.length - 1; i >= 0; i -= 1) {
      param = params_arr[i].split("=")[0];
      if (param === key) {
        params_arr.splice(i, 1);
      }
    }
    rtn = rtn + "?" + params_arr.join("&");
  }
  return rtn;
}

$(document).ready(function () {

  // if url includes my page name with id
  if (window.location.href.indexOf("pagename#wood") > -1) {

    // add class active to div which has id of wood and remove active class from other siblings
    $('#wood').addClass("last").siblings().removeClass('last');

    // add class active to tab content div which has id of wood-content and remove active class from other siblings
    $('#wood-content').addClass("content-active").siblings().removeClass('content-active');
  }
  //mobile view category list
  //$(".header__sublistWrapMobile").append($(".alchub-mobile"));

});

$(document).ready(function (e) {
  $(".enter-delivery-address").click(function () {
    $('.enter-delivery-address-wrap').toggleClass('open-popup');
  });

  $(".enter-delivery-address-backdrop").click(function () {
    $('.enter-delivery-address-wrap').removeClass('open-popup');
  });

  $(".textbox").focus(function () {
    $(this).parent().addClass("changelabel");


  }).blur(function () {
    $(this).parent().removeClass("changelabel");

  });
});


$(document).ready(function () {
  $(".discount-wrapper").click(function () {
    $(".listbox").slideToggle('slow');
    $(".discount-wrapper ").toggleClass("open");
  });
});



$('#category-slider').owlCarousel({
  loop: false,
  margin: 12,
  slideBy: 1,
  dotsEach: 1,
  dots: true,
  nav: true,

  responsive: {
    0: {
      items: 3,
      dots: true,
      nav: false,
    },

    379: {
      items: 4,
      dots: true,
      nav: false,
    },
    600: {
      items: 3,
      dots: true,
      nav: false,
    },
    1000: {
      items: 6
    },
    1300: {
      items: 11

    }
  }
});



//manufacture list slider
$('#brand-slider').owlCarousel({
  loop: false,
  margin: 20,

  dots: true,
  dotsEach: 1,
  responsive: {
    0: {
      items: 2,
      dots: false,
      /*dotsEach: 100,*/
      nav: true,

    },
    600: {
      items: 3,
      dots: false,
      /*  dotsEach: 100,*/
      nav: true,
    },
    810: {
      items: 3,
      dots: false,
      nav: true,
    },
    1000: {
      items: 6,
      nav: true,
    },
    1400: {
      items: 7,
      nav: true,
    }
  }
});

$(" .filters-button").on("click", function () {
  $("body").toggleClass("open-filters");
  $("body").addClass("open-filters");
  $(".overlayOffCanvas").addClass("active");
  $(".header").addClass("index");
  $(".header-lower").addClass("index");
  $(".header-lower-wrapper").css("z-index", "0");
  $(".footer").css("z-index", "-1");

});



$(".close-side-menu-btn").on("click", function () {
  $("body").removeClass("open-filters");
  $(".overlayOffCanvas").removeClass("show");
  $(".header").removeClass("index");
  $(".header-lower").removeClass("index");
  $(".overlayOffCanvas").removeClass("active");
  $(".footer").css("z-index", "1");
})


$(window).resize(function () {
  if (window.innerWidth > 940) {
    $("body").removeClass("open-filters");
    $(".overlayOffCanvas").removeClass("show");
  }
});



// When the user clicks on the button, scroll to the top of the document
var btn = $('#myBtn');
$(window).scroll(function () {
  if ($(window).scrollTop() > 300) {
    btn.fadeIn();
  } else {
    btn.fadeOut();
  }
});
function topFunction() {
  document.body.scrollTop = 0;
  document.documentElement.scrollTop = 0;
}


$(".product-filter .filter-title, .product-spec-group .name").click(function () {
  $(this).next().slideToggle('slow');
  $(this).toggleClass("closed")
});

$(document).ready(function () {
  $(' .loction-btn').on('click', function () {
    $('.drop-down').slideToggle();
  })

});

//display add review form on click
$('#write-review').click(function () {
  $('#write-review-form').slideToggle("slow");
  $('.title').addClass('open');
});
$(document).on('click', '.close', function (e) {
  e.preventDefault();
  $('.title').removeClass('open');
  $('write-review').toggle();
  $('#write-review-form').slideToggle("slow");
});

$('.qty-input').keypress(function (e) {
  var regex = new RegExp("^[0-9_]+$");
  var str = String.fromCharCode(!e.charCode ? e.which : e.charCode);
  if (regex.test(str)) {
    return true;
  }
  e.preventDefault();
  return false;
});


//mobile-responsive category section

$(document).on('click', '.overlayOffCanvas', function () {
  $('.header__mobileMenuWrapper').removeClass('active');
  $('.mobileSearch').removeClass('active');
  $(this).removeClass('active');
  $('.firstLevel').removeClass('active');
  $('.mobilecartPopup .cartPopup').removeClass('active');
  $('.mobileAccountBox').removeClass('active');
  $('.header__mobileSelecter').removeClass('openLevel');

});

$(document).on('click', '.alchub-category', function () {
  $(this).next().addClass('active');
  //$('.firstLevel').addClass('active');
});


$(document).on('click', '.header__plusButton', function () {

  $(this).parent().toggleClass('openLevel');
});
$(document).on('click', '.header__backButton', function () {
  $(this).parent().parent().parent().removeClass('openLevel');
  $(".firstLevel ").removeClass('active');
});


$(document).on('click', '.sidebar-button', function () {

  $('.overlayOffCanvas').addClass('active');
});
$(document).on('click', '.Mobileheader__closeMenu', function () {

  $('.overlayOffCanvas').removeClass('active');
  $('.header').removeClass('bg-color')
});


$(document).ready(function () {
  if ($(window).width() >= 500) {
    $(window).scroll(function fix_element() {
      $('.loction-input-wrapper').css(
        $(window).scrollTop() > 100
          ? { 'position': 'sticky', 'top': '50px' }
          : { 'position': 'relative', 'top': 'auto' }
      );
      return fix_element;
    }());
  }
  else {
    $(window).scroll(function fix_element() {
      $('.loction-input-wrapper').css(
        $(window).scrollTop() > 100
          ? { 'position': 'fixed', 'top': '39px' }
          : { 'position': 'relative', 'top': 'auto' }
      );
      return fix_element;
    }());
  }


  //text-elipse dots jquery
  const modules = document.querySelectorAll('.product-title-clamp');

  // Make sure our query found anything
  if (modules) {
    // Loop through each module and apply the clamping.
    modules.forEach((module, index) => {
      $clamp(module, { clamp: 2 });
    });
  }

  /* const list = document.querySelectorAll('.group .item label');
   if (list) {
     // Loop through each module and apply the clamping.
     list.forEach((module, index) => {
       $clamp(module, { clamp: 1 });
     });
   }*/

});

//slot Box design 
$(document).ready(function () {

  $(document).on('click', '.font-weight-bold', function () {

    $("body").css({ 'overflow-y': 'visible', 'padding-right': '0px' });

  });

  $(".container-wrap:lt(6)").addClass("review-wrp");

  $(document).on('click', '.see-more-review', function () {
    $(".container-wrap").addClass("review-wrp");
    $(".see-more-review").hide();
  })

  if ($('.review-box-custome').find('.container-wrap').length > 6) {
    $(".see-more-review").show();
  }
  else {
    $(".see-more-review").hide();
  }
});






//show text read more and read less

// Show more text option 
var showChar = 190;  // How many characters are shown by default
var ellipsestext = "...";
var moretext = "Read more";
var lesstext = "Read less";

//Cut content based on showChar length
if ($(".toggle-text").length) {
  $(".toggle-text").each(function () {

    var content = $(this).html();
    if (content.length > showChar) {

      var contentExcert = content.substr(0, showChar);
      var contentRest = content.substr(showChar, content.length - showChar);
      var html = contentExcert + '<span class="toggle-text-ellipses">' + ellipsestext + ' </span> <span class="toggle-text-content"><span>' + contentRest + '</span><a href="javascript:;" class="toggle-text-link">' + moretext + '</a></span>';

      $(this).html(html);
    }
  });
}

//Toggle content when click on read more link
$(".toggle-text-link").click(function () {
  if ($(this).hasClass("less")) {
    $(this).removeClass("less");
    $(this).html(moretext);
  } else {
    $(this).addClass("less");
    $(this).html(lesstext);
  }
  $(this).parent().prev().toggle();
  $(this).prev().toggle();
  return false;
});

$(document).ready(function () {


  if ($(window).width() <= 340) {
    $("#mobile-nivo-slider img").css('max-height', '150px');

  }

});

//Product details page
function disableAddToCart(disable) {
  var addToCartBtn = $("button.add-to-cart-button");
  if (addToCartBtn.length > 0) {
    if (disable === true) {
      //disable add to cart
      addToCartBtn.addClass("disableAddToCart");
    } else {
      //enable add to cart
      addToCartBtn.removeClass("disableAddToCart");
    }
  }
}