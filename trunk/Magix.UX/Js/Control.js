/*
 * Magix UX - A Managed Ajax Library for ASP.NET
 * Copyright 2010 - 2014 - isa.lightbringer@gmail.com
 * Magix is licensed as MITx11, see enclosed License.txt File for Details.
 */

(function() {

  MUX.Control = MUX.klass();


  MUX.C = function(el, opt) {
    return new MUX.Control(el, opt);
  };


  MUX.Control._ctrls = [];


  MUX.Control.errorHandler = function(stat, trc) {
    if (stat !== 0) {
      if (confirm('Oops...! ERROR! Want to check it out...?')) {
        var errDiv = document.createElement('div');
        MUX.extend(errDiv, MUX.Element.prototype);
        errDiv.setStyles({
          position: 'fixed',
          textAlign: 'center',
          top: '0px',
          left: '0px',
          width: '100%',
          height: '100%',
          zIndex: 10000
        });

        var close = document.createElement('div');
        MUX.extend(close, MUX.Element.prototype);
        close.setContent('Close');
        close.setStyles({
          position: 'relative',
          top: '20px',
          zIndex: '1000',
          cursor: 'pointer',
          textDecoration: 'underline',
          color: '#22e'
        });
        var functor = function() {
          close.stopObserving('click', functor, this);
          document.body.removeChild(errDiv);
        };
        close.observe('click', functor, this);

        var frame = document.createElement('iframe');
        MUX.extend(frame, MUX.Element.prototype);
        frame.setStyles({
          width: '95%',
          height: '95%',
          border: 'dashed 1px #000'
        });

        errDiv.appendChild(close);
        errDiv.appendChild(frame);
        document.body.appendChild(errDiv);

        var doc = frame.contentDocument || frame.contentWindow.document;

        doc.open();
        doc.write(trc);
        doc.close();
      }
    }
  };


  MUX.Control.$ = function(id) {
    var ctrls = MUX.Control._ctrls;
    var idx = ctrls.length;
    while (idx--) {
      if (ctrls[idx].element && ctrls[idx].element.id == id) {
        return ctrls[idx];
      }
    }
    return null;
  };


  MUX.Control.prototype = {

    init: function(el, opt) {
      this.initControl(el, opt);
    },

    initControl: function(el, opt) {
      this.element = MUX.$(el);
      this.options = MUX.extend({
        evts: [],
        focus: false,
        select: false,
        defaultWidget: null
      }, opt || {});

      if (this.options.defaultWidget) {
        this.element.observe('keypress', this.onKeyPressCheckEnter, this);
      }

      // Setting focus to control (of we should)
      if (this.options.focus) {
        this.Focus();
      }

      // Selecting contents of control (if we should)
      if (this.options.select) {
        this.element.select();
      }

      // Registering control
      MUX.Control._ctrls.push(this);

      // Creating event handlers for the client-side events needed to be dispatched 
      // back to server
      this.initEvents();
    },

    onKeyPressCheckEnter: function(evt) {
      if (evt.keyCode == 13) {
        var el = MUX.$(this.options.defaultWidget);
        el.focus();
        el.click();
        return false;
      }
    },

    // This is the method being called from the server-side when
    // we have messages sent to this control.
    // To handle specific data transfers for your controls
    // create a method with the exact same name as the "key" value
    // of the JSON value in your overridden Control class.
    // Normally these methods should be easy to spot in an extended control
    // since they should (by convention) start with a CAPITAL letter to
    // mimick the looks of a property...
    JSON: function(json) {

      // Looping through all "top-level" objects and calling the functions for those keys
      for (var idxKey in json) {
        this[idxKey](json[idxKey]);
      }
    },



    // JSON parser methods, called by server through the JSON function
    // These functions are easy to spot since they all starts with a CAPITAL 
    // letter (by convention) and they all take ONE parameter.


    // Expects only a string
    Class: function (val) {
      this.element.className = val;
    },

    // Expects and array of arrays where each array-item is a key/value object
    // and the key (first sub-item in array) is the name of the style property 
    // and the value (second sub-item array) its value
    // Note you can also use this one to REMOVE styles by having an empty string 
    // as the "value" part.
    AddStyle: function(val) {
      for (var idx = 0; idx < val.length; idx++) {
        this.element.setStyle(val[idx][0], val[idx][1]);
      }
    },

    // Expects only a string, does a replace on the innerHTML with the updated text string
    // Useful for labels, textareas and so on...
    InnerHtml: function(val) {
      this.element.setContent(val);
    },

    // Expects a single character - Sets the access key (ALT + value) for giving focus to control
    AccessKey: function(val) {
      this.element.accesskey = val;
    },

    // Expects a text value, sets the "value" of the control to the given value
    // Useful for TextBoxes (input type="text") and so on...
    Value: function(val) {
      this.element.value = val;
    },

    // Expects a text value, sets the "value" of the control to the given value
    // Useful for TextBoxes (input type="text") and so on...
    Dir: function(val) {
      this.element.dir = val;
    },

    // Sets focus to control
    Focus: function() {
      // Silently catching since Focus fails in IE (and throws) if DOM node (or ancestor node) is "display:none"...
      try { this.element.focus(); } catch (e) { }
    },

    // Selects a range from e.g. a TextBox
    Select: function() {
      this.element.select();
    },

    // Expects a type - defines type of control (text, password etc...)
    Type: function(val) {
      this.element.type = val;
    },

    // Expects any value, will set that property of the element
    // Useful for sending "generic" attributes over to the element
    Generic: function(val) {
      for (var idx = 0; idx < val.length; idx++) {
        this.element[val[idx][0]] = val[idx][1];
      }
    },



    // Initializes all events on control
    initEvents: function() {
      var evts = this.options.evts;
      var idx = evts.length;
      while (idx--) {
        if (evts[idx].length > 1 && evts[idx][1] == 'effect') {
          this.element.observe(evts[idx][0], function() {
            this.execute();
          }, evts[idx][2]);
        } else {
          switch (evts[idx][0]) {
            case 'enter':
              this.element.observe('keypress', this.onCheckEnter, this);
              break;
            case 'esc':
              this.element.observe('keydown', this.onCheckEsc, this);
              break;
            case 'keypress':
              this.element.observe('keypress', this.onKeyPress, this);
              this.element.observe('keyup', this.onKeyUp, this);
              break;
            default:
              // This one will prioritize the third event parameter, then the ctrl option and finally
              // the this.element if the two previous was undefined or not given
              (evts[idx].length > 2 ? MUX.$(evts[idx][2]) : this.element).observe(
              evts[idx][0],
              this.onEvent,
              this,
              [evts[idx][0], evts[idx][1]]);
              break;
          }
        }
      }
    },

    onCheckEnter: function(evt) {
      if (evt.keyCode == 13) {
        this.callback('enter');
        return false;
      }
    },

    onCheckEsc: function(evt) {
      if (evt.keyCode == 27) {
        this.callback('esc');
        return false;
      }
    },

    onKeyUp: function(evt) {
      /*var charCode = evt.which ? evt.which : evt.keyCode;
      if(charCode == 8 || charCode == 46 || charCode == 32) {
        this.onKeyPress();
      }*/
    },

    onKeyPress: function(evt) {
      var T = this;
      setTimeout(function() {
        if(T.element.value == T._xiTmrVal) {
          return;
        }
        T._xiTmrVal = T.element.value;
        if(!T._xiTmr) {
          T._xiTmr = setTimeout(
            function() {
              T.callBackForKeyPress();
            }, 500);
        } else {
          clearTimeout(T._xiTmr);
          T._xiTmr = setTimeout(
            function() {
              T.callBackForKeyPress();
            }, 500);
        }
      }, 1);
    },

    callBackForKeyPress: function() {
      this.onEvent('keypress', false);
    },

    getValue: function() {
      return this.element.value;
    },

    // Called when an event is raised, the parameter passed is the this.options.serverEvent instance 
    // which we will use to know how to call our server
    onEvent: function(evt, stop) {
      var T = this;
      this.callback(evt);
      return !stop;
    },

    callback: function(evt, func, params) {
      var T = this;
      var arguments = 'magix.ux.callback-control=' + this.element.id + '&magix.ux.event-name=' + evt;
      if (params) {
        for (var idx = 0; idx < params.length; idx++) {
          arguments += '&' + params[idx]['name'] + '=' + params[idx]['value'];
        }
      }
      var x = new MUX.Ajax({
        args: arguments,
        onSuccess: function(resp) {
          try {
            T.onFinishedRequest(resp);
            if (func) {
              func();
            }
          } catch (e) {
            alert(e); // Nothing else to do here. Just inform user of JS bug through alert...
          }
        },
        onError: function(stat, trc) {
          T.onFailedRequest(stat, trc);
          if (func) {
            func();
          }
        },
        callingContext: this
      });
    },

    onFinishedRequest: function(resp) {
      if (this._oldValue) {
        delete this._oldValue;
      }
      this._xiTmr = null;
      eval(resp);
    },

    onFailedRequest: function(stat, trc) {
      if (this._oldValue) {
        delete this._oldValue;
      }
      MUX.Control.errorHandler(stat, trc);
    },

    reRender: function(html) {
      this._reRender(html);
    },

    _reRender: function(html) {
      this._destroyChildControls();
      this._unlistenEventHandlers();
      this.element = this.element.repl(html);
      this.initEvents();
    },

    destroy: function(ht) {
      if(this.preDestroyer) {
        this.preDestroyer();
      }
      this._destroyChildControls();
      this.destroyThis();
      if (!ht) {
        this.element.repl("<span id=\"" + this.element.id + "\" style=\"display:none;\" />");
      } else {
        this.element.repl(ht);
      }
    },

    _destroyChildControls: function() {
      var children = [];
      var ctrls = MUX.Control._ctrls;
      var idx = ctrls.length;
      while (idx--) {
        var tId = this.element.id;
        if (ctrls[idx].element && ctrls[idx].element.id.indexOf(tId) === 0 &&
        ctrls[idx].element.id.substring(tId.length, tId.length + 1) == '_') {
          children.push(ctrls[idx]);
        }
      }
      var i = children.length;
      while (i--) {
        children[i].destroyThis();
      }
    },

    destroyThis: function() {
      this._destroyThisControl();
    },

    _destroyThisControl: function() {
      this._unlistenEventHandlers();

      var ctrls = MUX.Control._ctrls;
      idx = ctrls.length;
      while (idx--) {
        if (ctrls[idx].element && ctrls[idx].element.id == this.element.id) {
          break;
        }
      }
      ctrls.splice(idx, 1);
    },

    _unlistenEventHandlers: function() {
      var evts = this.options.evts;
      var idx = evts.length;
      while (idx--) {
        switch (evts[idx][0]) {
          case 'enter':
            this.element.stopObserving('keypress', this.onCheckEnter, this);
            break;
          case 'esc':
            this.element.stopObserving('keydown', this.onCheckEsc, this);
            break;
          default:
            this.element.stopObserving(evts[idx][0], this.onEvent, this);
            break;
        }
      }
    }
  };

  MUX.Control.callServerMethod = function(method, opt, args) {
    var mArgs = 'magix.ux.function-name=' + method;

    opt = MUX.extend({
      onSuccess: function() { },
      onError: function() { }
    }, opt || {});

    if (args) {
      for (var idx = 0; idx < args.length; idx++) {
        mArgs += '&magix.ux.function-parameter' + idx + '=' + args[idx];
      }
    }

    MUX.Control._functionReturnValue = null;
    var x = new MUX.Ajax({
      args: mArgs,
      onSuccess: function(resp) {
        eval(resp);
        opt.onSuccess(MUX.Control._functionReturnValue);
      },
      onError: function(stat, trc) {
        opt.onError(stat, trc);
      }
    });
  };

})();
