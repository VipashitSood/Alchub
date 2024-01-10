/*
** nopCommerce ajax cart implementation
*/


var AjaxCart = {
  loadWaiting: false,
  usepopupnotifications: false,
  topcartselector: '',
  topwishlistselector: '',
  flyoutcartselector: '',
  localized_data: false,

  init: function (usepopupnotifications, topcartselector, topwishlistselector, flyoutcartselector, localized_data) {
    this.loadWaiting = false;
    this.usepopupnotifications = usepopupnotifications;
    this.topcartselector = topcartselector;
    this.topwishlistselector = topwishlistselector;
    this.flyoutcartselector = flyoutcartselector;
    this.localized_data = localized_data;
  },

  setLoadWaiting: function (display) {
    displayAjaxLoading(display);
    this.loadWaiting = display;
  },

  //add a product to the cart/wishlist from the catalog pages
  addproducttocart_catalog: function (urladd) {
    if (this.loadWaiting !== false) {
      return;
    }
    this.setLoadWaiting(true);

    var postData = {};
    addAntiForgeryToken(postData);

    $.ajax({
      cache: false,
      url: urladd,
      type: "POST",
      data: postData,
      success: this.success_process,
      complete: this.resetLoadWaiting,
      error: this.ajaxFailure
    });
  },

  //add a product to the cart/wishlist from the product details page
  addproducttocart_details: function (urladd, formselector) {
    if (this.loadWaiting !== false) {
      return;
    }
    this.setLoadWaiting(true);

    $.ajax({
      cache: false,
      url: urladd,
      data: $(formselector).serialize(),
      type: "POST",
      beforeSend: function () { },
      success: this.success_process,
      complete: this.resetLoadWaiting,
      error: this.ajaxFailure
    });
  },

  //add a product to compare list
  addproducttocomparelist: function (urladd) {
    if (this.loadWaiting !== false) {
      return;
    }
    this.setLoadWaiting(true);

    var postData = {};
    addAntiForgeryToken(postData);

    $.ajax({
      cache: false,
      url: urladd,
      type: "POST",
      data: postData,
      success: this.success_process,
      complete: this.resetLoadWaiting,
      error: this.ajaxFailure
    });
  },

  //add a product to the cart/wishlist from the catalog pages
  deleteproductfromcart_shoppingcart: function (urladd) {
    if (this.loadWaiting !== false) {
      return;
    }
    this.setLoadWaiting(true);

    var postData = {};
    addAntiForgeryToken(postData);

    $.ajax({
      cache: false,
      url: urladd,
      type: "POST",
      data: postData,
      success: function (response) {
        if (response.updateWishlistHeartProductId) {
          $(".wishlistlink_" + response.updateWishlistHeartProductId).removeClass('active');
          $(".wishlistlink_" + response.updateWishlistHeartProductId).attr("onclick", "return AjaxCart.addproducttocart_catalog(" + '"' + "/addproducttocart/catalog/" + response.updateWishlistHeartProductId + "/2/1" + '"' + "),!1");
        }
        if (response.updatetopwishlistsectionhtml)
        {
          if (response.updatetopwishlistsectionhtml == 0) {
            $('.wishlist-qty').removeClass('active');
            $('.wishlist-qty').addClass('Inactive');
            $(AjaxCart.topwishlistselector).html('');
          }
          else {
            $('.wishlist-qty').removeClass('Inactive');
            $('.wishlist-qty').addClass('active');
            $(AjaxCart.topwishlistselector).html(response.updatetopwishlistsectionhtml);
          }
        }

        displayBarNotification(response.message, 'error', 3500);
      },
      complete: this.resetLoadWaiting,
      error: this.ajaxFailure
    });
  },

  success_process: function (response) {
    if (response.updatetopcartsectionhtml) {
      if (response.updatetopcartsectionhtml == 0)
      {
        $('.cart-qty').removeClass('active');
        $('.cart-qty').addClass('Inactive');
        $(AjaxCart.topcartselector).html('');
      }
      else
      {
        $('.cart-qty').removeClass('Inactive');
        $('.cart-qty').addClass('active');
        $(AjaxCart.topcartselector).html(response.updatetopcartsectionhtml);
      }
      
    }
    if (response.updatetopwishlistsectionhtml) {
      if (response.updatetopwishlistsectionhtml == 0) {
        $('.wishlist-qty').removeClass('active');
        $('.wishlist-qty').addClass('Inactive');
        $(AjaxCart.topwishlistselector).html('');
      }
      else {
        $('.wishlist-qty').removeClass('Inactive');
        $('.wishlist-qty').addClass('active');
        $(AjaxCart.topwishlistselector).html(response.updatetopwishlistsectionhtml);
      }
    }
    if (response.updateflyoutcartsectionhtml) {
      $(AjaxCart.flyoutcartselector).replaceWith(response.updateflyoutcartsectionhtml);
    }
    if (response.updateWishlistHeartProductId) {
      $(".wishlistlink_" + response.updateWishlistHeartProductId).addClass('active');
      $(".wishlistlink_" + response.updateWishlistHeartProductId).attr("onclick", "AjaxCart.deleteproductfromcart_shoppingcart('/deleteproductfromcart/details/" + response.updateWishlistHeartProductId + "/2', '#product-details-form_" + response.updateWishlistHeartProductId + "');return false;");
    }
    if (response.message) {
      //display notification
      if (response.success === true) {
        //success
        if (AjaxCart.usepopupnotifications === true) {
          displayPopupNotification(response.message, 'success', true);
        }
        else {
          //specify timeout for success messages
          displayBarNotification(response.message, 'success', 3500);
        }
      }
      else {
        //error
        if (AjaxCart.usepopupnotifications === true) {
          displayPopupNotification(response.message, 'error', true);
        }
        else {
          //no timeout for errors
          displayBarNotification(response.message, 'error', 0);
        }
      }
      return false;
    }
    if (response.redirect) {
      location.href = response.redirect;
      return true;
    }
    return false;
  },

  resetLoadWaiting: function () {
    AjaxCart.setLoadWaiting(false);
  },

  ajaxFailure: function () {
    alert(this.localized_data.AjaxCartFailure);
  }
};