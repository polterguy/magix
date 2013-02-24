/*
* Magix UX - A Managed Ajax Library for ASP.NET
* Copyright 2010 - 2013 - MareMara13@gmail.com
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
      if (this.options.enabled)
        this.start();
    },

    start: function() {
      var T = this;
      this._timer = setTimeout(function() {
        T.tick();
      }, this.options.interval);
    },

    tick: function() {
      if (this.options.enabled) {
        this.callback();
      }
    },

    callback: function() {
      var x = new MUX.Ajax({
        args: '__MUX_CONTROL_CALLBACK=' + this.element.id + '&__MUX_EVENT=tick',
        onSuccess: this.onFinishedTicking,
        callingContext: this
      });
    },

    Enabled: function(value) {
      if (value && !this.options.enabled) {
        this.start();
      }
      this.options.enabled = value;
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
      if (this.options.enabled) {
        this.start();
      }
    },

    destroyThis: function() {
      // To make sure the next 'tick' never goes through ...!
      this.options.enabled = false;
      this._destroyThisControl();
    }
  });
})();
