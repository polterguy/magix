/*
* Magix - A Managed Ajax Library for ASP.NET
* Copyright 2010 - 2012 - QueenOfSpades20122@gmail.com
* Magix is licensed as MITx11, see enclosed License.txt File for Details.
*/

(function() {

  MUX.Wheel = MUX.klass();

  MUX.extend(MUX.Wheel.prototype, MUX.Control.prototype);

  MUX.extend(MUX.Wheel.prototype, {
    init: function(el, opt) {
      this.initControl(el, opt);
      this.options = MUX.extend({
      }, this.options || {});

      var el = this.element;
      if (el.firstChild) {
        MUX.$(el.firstChild);
        var b = MUX.$(document.body);
        el.observe('touchstart', this.onTouchstart, this);
        el.observe('mousedown', this.onTouchstart, this);

        b.observe('touchmove', this.onTouchmove, this);
        b.observe('mousemove', this.onTouchmove, this);

        b.observe('touchend', this.onTouchend, this);
        b.observe('mouseup', this.onTouchend, this);
      }
    },

    onTouchstart: function(evt){
      this._beginPtrPos = this.pointer(evt);
      this._beginY = parseInt(this.element.firstChild.getStyle('marginTop'), 10) || 0;
      this._hasLock = true;
    },

    onTouchmove: function(evt){
      if (this._hasLock) {
        var ptrPos = this.pointer(evt);
        var delY = this._beginY + ptrPos.y - this._beginPtrPos.y;
        delY -= delY % 18;
        delY = Math.min(delY, 36);
        if ((-delY) > (this.element.childNodes.length - 3) * 18) {
          delY = -((this.element.childNodes.length - 3) * 18);
        }
        this.element.firstChild.setStyle('marginTop', delY + 'px');
      }
    },

    onTouchend: function(evt){
      if (this._hasLock) {
        this._hasLock = false;
        this.callback();
      }
    },

    callback: function() {
      this._beginY = parseInt(this.element.firstChild.getStyle('marginTop') || 0, 10);
      var x = new MUX.Ajax({
        args: '__MUX_CONTROL_CALLBACK=' + this.element.id + '&__MUX_EVENT=selectedChanged&__sel='+((-this._beginY / 18) + 2),
        onSuccess: this.onFinishedRequest,
        callingContext: this
      });
    },

    pointer: function(event) {
      return {
        x: event.pageX ||
          (event.clientX + (document.documentElement.scrollLeft || document.body.scrollLeft)),
        y: event.pageY ||
          (event.clientY + (document.documentElement.scrollTop || document.body.scrollTop))
      };
    },

    destroyThis: function() {
      var el = this.element;
      var b = MUX.$(document.body);

      el.stopObserving('touchstart', this.onTouchstart, this);
      el.stopObserving('mousedown', this.onTouchstart, this);
      b.stopObserving('touchmove', this.onTouchmove, this);
      b.stopObserving('mousemove', this.onTouchmove, this);
      b.stopObserving('touchend', this.onTouchend, this);
      b.stopObserving('mouseup', this.onTouchend, this);
      this._destroyThisControl();
    }
  });
})();
