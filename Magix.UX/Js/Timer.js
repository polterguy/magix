/*
* Magix UX - A Managed Ajax Library for ASP.NET
* Copyright 2010 - 2014 - thomas@magixilluminate.com
* Magix is licensed as MITx11, see enclosed License.txt File for Details.
*/

(function() {

  MUX.Timer = MUX.klass();

  MUX.extend(MUX.Timer.prototype, MUX.Control.prototype);

  MUX.extend(MUX.Timer.prototype, {
    init: function(el, opt) {
      this.initControl(el, opt);
      this.options = MUX.extend({
        interval: 1000
      }, this.options || {});
      if (!this.options.disabled)
        this.start();
    },

    start: function() {
      var T = this;
      this._timer = setTimeout(function() {
        T.tick();
      }, this.options.interval);
    },

    tick: function() {
      if (!this.options.disabled) {
        this.callback();
      }
    },

    callback: function() {
      var x = new MUX.Ajax({
          args: 'magix.ux.callback-control=' + this.element.id + '&magix.ux.event-name=tick',
        onSuccess: this.onFinishedTicking,
        callingContext: this
      });
    },

    Disabled: function(value) {
      if (!value && this.options.disabled) {
        this.start();
      }
      this.options.disabled = value;
    },

    Restart: function() {
      clearTimeout(this._timer);
      this.start();
    },

    Interval: function(value) {
      this.options.interval = value;
    },

    onFinishedTicking: function(response) {
      this.onFinishedRequest(response);
      if (!this.options.disabled) {
        this.start();
      }
    },

    destroyThis: function() {
      // To make sure the next 'tick' never goes through ...!
      this.options.disabled = true;
      this._destroyThisControl();
    }
  });
})();
