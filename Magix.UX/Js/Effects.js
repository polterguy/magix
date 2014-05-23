/*
 * Magix UX - A Managed Ajax Library for ASP.NET
 * Copyright 2010 - 2014 - thomas@magixilluminate.com
 * Magix is licensed as MITx11, see enclosed License.txt File for Details.
 */

(function() {

  MUX.Effect = MUX.klass();
  MUX.Effect.prototype = {
    init: function(el, opt) {
      this.initEffect(el, opt);
    },

    initEffect: function(el, opt) {
      this.options = MUX.extend({
        duration: 1000,
        onStart: function() { },
        onFinished: function() { },
        condition: function() { return true; },
        onRender: null,
        transition: 'Linear',
        autoStart: true
      }, opt || {});
      if (el) {
        this.element = MUX.$(el);
      }
      if (this.options.autoStart) {
        this.execute();
      }
    },

    execute: function() {
      if (!this.options.condition.call(this)) {
        return;
      }
      this.options.onStart.call(this);
      this.startTime = new Date().getTime();
      this.finishOn = this.startTime + (this.options.duration);
      this.loop();
    },

    loop: function() {
      if (this.stopped) {
        return;
      }
      var T = this;

      setTimeout(function() {
        var cur = new Date().getTime();
        var dlt = (cur - T.startTime) / (T.options.duration);
        if (cur >= T.finishOn) {
          T.options.onFinished.call(T);
        } else {
          T.render(dlt);
          T.loop();
        }
      }, 10);
    },

    render: function(pos) {
      switch (this.options.transition) {
        case 'Linear':
          break;
        case 'Accelerating':
          pos = Math.cos((pos * (Math.PI / 2)) + Math.PI) + 1;
          break;
        case 'Explosive':
          pos = Math.sin(pos * (Math.PI / 2));
          break;
      }
      this.options.onRender.call(this, pos);
    }
  };


  // Common for all Client-Side effects...
  MUX.Effect.ClientSide = MUX.klass();
  MUX.extend(MUX.Effect.ClientSide.prototype, MUX.Effect.prototype);
  MUX.extend(MUX.Effect.ClientSide.prototype, {

    init: function(el, opt) {
      this.initC(el, opt);
    },

    initC: function(el, opt) {
      this.options = MUX.extend({
        onStart: this.onStart,
        onFinished: this.onFinished,
        onRender: this.onRender,
        joined: []
      }, opt || {});
      this.initEffect(el, this.options);
    },

    onStart: function() {
      this.doStart();
      var e = this.options.joined;
      var idx = e.length;
      while (idx--) {
        e[idx].doStart();
      }
    },

    onRender: function(pos) {
      this.doRender(pos);
      var e = this.options.joined;
      var idx = e.length;
      while (idx--) {
        e[idx].doRender(pos);
      }
    },

    onFinished: function() {
      this.doFinish();
      var e = this.options.joined;
      var idx = e.length;
      while (idx--) {
        e[idx].doFinish();
      }
      if (this.options.chained) {
        this.options.chained.execute(this);
      }
    }
  });

  // Effect Opacity...
  MUX.Effect.Opacity = MUX.klass();
  MUX.extend(MUX.Effect.Opacity.prototype, MUX.Effect.ClientSide.prototype);
  MUX.extend(MUX.Effect.Opacity.prototype, {

    init: function(el, opt) {
      this.initOpacity(el, opt);
    },

    initOpacity: function(el, opt) {
      this.options = MUX.extend({
        from: 0,
        to: 1.0
      }, opt || {});
      this.initC(el, this.options);
    },

    doStart: function() {
      this.element.setOpacity(this.options.from);
      this.element.setStyle('display', '');
    },

    doRender: function(pos) {
      this.element.setOpacity(this.options.from + ((this.options.to - this.options.from) * pos));
    },

    doFinish: function() {
      this.element.setOpacity(this.options.to);
      if (this.options.to == 0) {
        this.element.setStyle('display', 'none');
      }
    }
  });

  // Effect Focus Select ...
  MUX.Effect.FocusSelect = MUX.klass();
  MUX.extend(MUX.Effect.FocusSelect.prototype, MUX.Effect.ClientSide.prototype);
  MUX.extend(MUX.Effect.FocusSelect.prototype, {

    init: function(el, opt) {
      this.options = MUX.extend({
        isFocus: false
      }, opt || {});
      this.initC(el, opt);
    },

    doStart: function() { },

    doRender: function() { },

    doFinish: function() {
      this.element.focus();
      if (this.element.select && !this.options.isFocus) {
        this.element.select();
      }
    }
  });

  // Effect Border...
  MUX.Effect.Highlight = MUX.klass();
  MUX.extend(MUX.Effect.Highlight.prototype, MUX.Effect.ClientSide.prototype);
  MUX.extend(MUX.Effect.Highlight.prototype, {

    init: function(el, opt) {
      this.initC(el, opt);
    },

    doStart: function() {
      this._startColor = this.element.getStyle('backgroundColor');
    },

    doRender: function(pos) {
      var a = 1.0 - pos;
      this.element.setStyle('backgroundColor', 'rgba(255, 255, 0,' + a);
    },

    doFinish: function() {
      this.element.setStyle('backgroundColor', this._startColor);
    }
  });

  // Effect Move...
  MUX.Effect.Move = MUX.klass();
  MUX.extend(MUX.Effect.Move.prototype, MUX.Effect.ClientSide.prototype);
  MUX.extend(MUX.Effect.Move.prototype, {

    init: function(el, opt) {
      this.initMove(el, opt);
    },

    initMove: function(el, opt) {
      this.options = MUX.extend({
        x: -1,
        y: -1
      }, opt || {});
      this.initC(el, this.options);
    },

    doStart: function() {
      this.startL = parseInt(this.element.getStyle('left'), 10) || 0;
      this.startT = parseInt(this.element.getStyle('top'), 10) || 0;
    },

    doRender: function(pos) {
      if (this.options.x != -1) {
        var deltaL = ((this.options.x) - this.startL) * pos;
        var newL = parseInt((deltaL) + this.startL, 10);
        this.element.setStyle('left', (newL) + 'px');
      }
      if (this.options.y != -1) {
        var deltaT = ((this.options.y) - this.startT) * pos;
        var newT = parseInt((deltaT) + this.startT, 10);
        this.element.setStyle('top', (newT) + 'px');
      }
    },

    doFinish: function() {
      if (this.options.x != -1) {
        this.element.setStyle('left', this.options.x + 'px');
      }
      if (this.options.y != -1) {
        this.element.setStyle('top', this.options.y + 'px');
      }
    }
  });

  // Effect RollUp...
  MUX.Effect.RollUp = MUX.klass();
  MUX.extend(MUX.Effect.RollUp.prototype, MUX.Effect.ClientSide.prototype);
  MUX.extend(MUX.Effect.RollUp.prototype, {

    init: function(el, opt) {
      this.initC(el, opt);
    },

    doStart: function() {
      this._fromHeight = this.element.dimensions().y;
      this._overflow = this.element.getStyle('overflow');
      this.element.setStyle('overflow', 'hidden');
      this.element.setStyle('display', 'block');
    },

    doRender: function(pos) {
      this.element.setStyle('height', ((1.0 - pos) * this._fromHeight) + 'px'); ;
    },

    doFinish: function() {
      this.element.setStyle('display', 'none');
      this.element.setStyle('height', '');
      if (this.options.overflow) {
        this.element.setStyle('overflow', this.options.overflow);
      } else {
        this.element.setStyle('overflow', this._overflow);
      }
    }
  });

  // Effect RollDown...
  MUX.Effect.RollDown = MUX.klass();
  MUX.extend(MUX.Effect.RollDown.prototype, MUX.Effect.ClientSide.prototype);
  MUX.extend(MUX.Effect.RollDown.prototype, {

    init: function(el, opt) {
      this.initC(el, opt);
    },

    doStart: function() {
      this._toHeight = this.element.dimensions().y;
      this.element.setStyle('height', '0px');
      this.element.setStyle('display', 'block');
      this._overflow_x = this.element.getStyle('overflowX');
      this._overflow_y = this.element.getStyle('overflowY');
      this.element.setStyle('overflow', 'hidden');
    },

    doRender: function(pos) {
      this.element.setStyle('height', parseInt(this._toHeight * pos, 10) + 'px');
    },

    doFinish: function() {
      this.element.setStyle('height', '');
      this.element.setStyle('overflowX', this._overflow_x);
      this.element.setStyle('overflowY', this._overflow_y);
    }
  });

  // Effect Size...
  MUX.Effect.Size = MUX.klass();
  MUX.extend(MUX.Effect.Size.prototype, MUX.Effect.ClientSide.prototype);
  MUX.extend(MUX.Effect.Size.prototype, {

    init: function(el, opt) {
      this.initSize(el, opt);
    },

    initSize: function(el, opt) {
      this.options = MUX.extend({
        x: -1,
        y: -1
      }, opt || {});
      this.initC(el, this.options);
    },

    doStart: function() {
      this.startSize = this.element.dimensions();
    },

    doRender: function(pos) {
      if (this.options.y != -1) {
        var deltaH = (this.options.y - this.startSize.y) * pos;
        var newH = parseInt(deltaH + this.startSize.y, 10);
        this.element.setStyle('height', newH + 'px');
      }
      if (this.options.x != -1) {
        var deltaW = (this.options.x - this.startSize.x) * pos;
        var newW = parseInt(deltaW + this.startSize.x, 10);
        this.element.setStyle('width', newW + 'px');
      }
    },

    doFinish: function() {
      if (this.options.y != -1) {
        this.element.setStyle('height', this.options.y + 'px');
      }
      if (this.options.x != -1) {
        this.element.setStyle('width', this.options.x + 'px');
      }
    }
  });

  // Effect Timeout...
  MUX.Effect.Timeout = MUX.klass();
  MUX.extend(MUX.Effect.Timeout.prototype, MUX.Effect.ClientSide.prototype);
  MUX.extend(MUX.Effect.Timeout.prototype, {

    init: function(el, opt) {
      this.initC(el, opt);
    },

    doStart: function() { },

    doRender: function() { },

    doFinish: function() { }
  });

  // Effect Slide...
  MUX.Effect.Slide = MUX.klass();
  MUX.extend(MUX.Effect.Slide.prototype, MUX.Effect.ClientSide.prototype);
  MUX.extend(MUX.Effect.Slide.prototype, {

    init: function(el, opt) {
      this.initSlide(el, opt);
    },

    initSlide: function(el, opt) {
      this.options = MUX.extend({
        offset: 0
      }, opt || {});
      this.initC(el, this.options);
    },

    doStart: function() {
      MUX.extend(this.element.parentNode, MUX.Element.prototype);
      this._width = this.element.parentNode.dimensions().x;
      this._startMargin = parseInt(this.element.getStyle('marginLeft'), 10);
    },

    doRender: function(pos) {
      var fullDelta = (this.options.offset * this._width) - this._startMargin;
      var n = parseInt(pos * fullDelta, 10);
      this.element.setStyle('marginLeft', (n + this._startMargin) + 'px');
    },

    doFinish: function() {
      this.element.setStyle('marginLeft', (this.options.offset * this._width) + 'px');
    }
  });
})();
