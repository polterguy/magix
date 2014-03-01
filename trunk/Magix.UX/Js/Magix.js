/*
 * Magix UX - A Managed Ajax Library for ASP.NET
 * Copyright 2010 - 2014 - isa.lightbringer@gmail.com
 * Magix is licensed as MITx11, see enclosed License.txt File for Details.
 */

(function() {

  // This entire file is deeply inspired from the works of Sam Stephenson and his 
  // prototype.js library
  // Thanx dude ... :)
  // But unfortunstely, to use your work directly would be way overkill and make 
  // the JS portions of MUX increase by almost an order of magnitude ... :(

  MUX = {};

  MUX.emptyFunction = function() { };

  MUX.Browser = {
    IE: window.attachEvent && !window.opera,
    Opera: !!window.opera,
    WebKit: navigator.userAgent.indexOf('AppleWebKit') != -1,
    Gecko: navigator.userAgent.indexOf('Gecko/') != -1,
    MobileSafari: !!navigator.userAgent.match(/Apple.*Mobile.*Safari/)
  };

  MUX.$ = function(id) {
    if (id._className == 'MUX.Element') {
      // Element is already an element, and it is already extended
      return id;
    } else if (id.parentNode) {
      // This is a DOM element, hence we're extending it and returning it as it is
      return MUX.extend(id, MUX.Element.prototype);
    }
    // Finding element
    var el = document.getElementById(id);
    if (!el) {
      // Oops, didn't find it ...
      return null;
    }
    // Extending it before returning it ...
    MUX.extend(el, MUX.Element.prototype);
    return el;
  };

  // Will retrieve an input form element and exchange its value from the given offset. 
  // If it doesn't exist an element will be created and given the specific ID
  // Used to modify the __VIEWSTATE
  MUX.$F = function(id, val, offset) {
    var el = document.getElementById(id);
    if (!el) {
      // Oops, element didn't exist. Need to create it...
      el = document.createElement('input');
      el.id = id;
      el.type = 'hidden';
      el.name = id;
      document.getElementsByTagName('form')[0].appendChild(el);
    }
    if (val) {
      el.value = el.value.substring(0, offset) + val;
    }
  };

  MUX._scripts = {};

  // Will synchronously retrieve a (JavaScript) document form the server and evaluate it
  // Basically "include" a JavaScript file. Will not include that file if it has been
  // included before which is defined as if it exists with a true value in the MUX._scripts 
  // list
  MUX.$I = function(url) {
    if (!MUX._scripts[url]) {
      var xhr = MUX.newXHR();
      xhr.open('GET', url, false);
      xhr.send('');
      if (xhr.status && xhr.status >= 200 && xhr.status < 300 && xhr.responseText) {
        window.eval.call(window, xhr.responseText);
        MUX._scripts[url] = true;
      } else {
        alert('Something went wrong while trying to include a JavaScript file from the server');
      }
    }
  };

  // Will create a new XHR object and return it back to caller
  MUX.newXHR = function() {
    return (window.XMLHttpRequest && new XMLHttpRequest()) ||
    new ActiveXObject('Msxml2.XMLHTTP') ||
    new ActiveXObject('Microsoft.XMLHTTP');
  };

  // Basically the OOP JavaScript "heart" of the Magix library. Will create a new
  // "class". Whenever an object is created, the init function, if it exists, will
  // be called on the instance.
  MUX.klass = function() {
    return function() {
      if (this.init) {
        return this.init.apply(this, arguments);
      }
    };
  };

  // Extends the LHS object with all properties from the RHS object and returns it to the caller.
  MUX.extend = function(lhs, rhs) {
    for (var prop in rhs) {
      lhs[prop] = rhs[prop];
    }
    return lhs;
  };


  /*
  * XMLHTTPRequest wrapper class.
  */
  MUX.XHR = MUX.klass();
  MUX.XHR._hasRequest = false;

  MUX.XHR.prototype = {

    _className: 'MUX.XHR',

    init: function(url, opt) {
      this.url = url;
      this.options = MUX.extend({
        onSuccess: MUX.emptyFunction,
        onError: MUX.emptyFunction,
        onTimeout: MUX.emptyFunction,
        body: '',
        autoStart: true,
        callingContext: this
      }, opt || {});
      if (MUX.XHR._hasRequest) {
        throw 'Oops...! Active request already in queue. Something went very wrong here...!';
      }
      if (this.options.autoStart) {
        this.send();
      }
    },

    send: function() {
      MUX.XHR._hasRequest = true;
      this.xhr = MUX.newXHR();
      this.xhr.open('POST', this.url, true);
      this.xhr.setRequestHeader('Accept', 'application/javascript');
      this.xhr.setRequestHeader('Content-Type', 'application/x-www-form-urlencoded; charset=UTF-8');
      this.xhr.overrideMimeType('application/javascript');
      var T = this;
      this.xhr.onreadystatechange = function() {
        if (T.xhr.readyState == 4) {
          T._done();
        }
      };
      this.xhr.send(this.options.body);
    },

    _done: function() {

      // Resettings so that next request can be initiated...
      MUX.XHR._hasRequest = false;

      if (this.xhr.responseText === null) {
        // Operation timed out...
        this.options.onTimeout();
      } else {
        // Some kind of success...
        if (this.xhr.status >= 200 && this.xhr.status < 300) {
          if (this.xhr.status == 222) {
            // Server sent us a redirect response...
            this.options.onSuccess.apply(this.options.callingContext, [this.xhr.responseText]);
            var headers = this.xhr.getAllResponseHeaders().split('\n');
            var idx = headers.length;
            while (idx--) {
              if (headers[idx].indexOf('Location') == 0) {
                var location = headers[idx].substr(10);
                window.location = location;
                return;
              }
            }
          } else {
            // Normal success response...
            this.options.onSuccess.apply(this.options.callingContext, [this.xhr.responseText]);
          }
        } else {
          // Some kind of error...
          this.options.onError.apply(this.options.callingContext, [this.xhr.status, this.xhr.responseText]);
        }
      }
    }
  };


  /*
  * Our Element class. 
  * Basically wraps a DOM element and 
  * gives it some basic functionality needed by Magix UX
  */
  MUX.Element = MUX.klass();
  MUX.Element._evtId = 1;
  MUX.Element._scriptRegEx = '<script[^>]*>([\\S\\s]*?)<\/script>';
  MUX.Element._cssRegEx = /<link\s+.*href\s*=\s*[\'\"](([^\'\"]+)?)[^>]*[>]+/img;

  // The only way to create instances of this class is by using the MUX.$ function
  // or yourself using the MUX.extend function.
  MUX.Element.prototype = {

    _className: 'MUX.Element',

    // innerHTML wrapper. Will strip and execute any scripts since some browsers don't support
    // automatic execution of scripts inside of html passed into innerHTML...
    setContent: function(val) {
      val = this.executeScripts(val);
      this.innerHTML = val;
    },

    // Will execute scripts in HTML and return HTML without the scripts.
    // Useful for complex HTML containing scripts in addition to HTML.
    executeScripts: function(val) {
      var rx = new RegExp(MUX.Element._scriptRegEx, 'img');
      var scr = rx.exec(val);
      var sf = false;
      while (scr) {
        sf = true;
        var TT = scr[1];
        window.eval.call(window, TT);
        scr = rx.exec(val);
      }
      if (sf) {
        return val.replace(rx, '');
      }
      return val;
    },

    // Will include any CSS inclusions into the header of the document, 
    // unless CSS is already included from before ...
    includeCSS: function(val) {
      var rx = MUX.Element._cssRegEx;
      var scr = rx.exec(val);
      var sf = false;
      while (scr) {
        sf = true;
        var cssL = document.getElementsByTagName('link');
        var found = false;
        for(var i = 0; i < cssL.length; i++) {
          if (cssL[i].href.indexOf(scr[1]) != -1
            && cssL[i].parentNode.tagName == 'head') {
            found = true;
            break;
          }
        }
        if (!found) {
          var el = document.createElement("link");
          var href = scr[1];
          el.href = href;
          el.rel = 'stylesheet';
          el.type = 'text/css';
          var head = document.getElementsByTagName('head')[0];

          if(href.indexOf('?back') != -1) {
            head.appendChild(el);
          } else {
            var frScr = null;
            for (var i = 0; i < head.children.length; i++) {
              var idxCh = head.children[i];
              if (idxCh.tagName == 'SCRIPT') {
                frScr = idxCh;
                break;
              }
              if(idxCh.tagName == 'LINK' && idxCh.href.indexOf('?back') != -1) {
                frScr = idxCh;
                break;
              }
            }
            head.insertBefore(el, frScr);
          }
        }
        scr = rx.exec(val);
      }
      if (sf) {
        return val.replace(rx, '');
      }
      return val;
    },

    // Replace entire element's HTML with given HTML
    // Will actually strip the scripts parts and execute them separately to 
    // support all the different browsers.
    repl: function(val) {
      if (!val) {
        // This is a removal operation...
        this.parentNode.removeChild(this);
      } else {
        // Content was given, hence replacing existing content with given content
        var id = this.id;
        val = this.executeScripts(val);
        val = this.includeCSS(val);
        if (this.outerHTML) {
          this.outerHTML = val;
        } else {
          var rn = this.ownerDocument.createRange();
          rn.selectNode(this);
          var el = rn.createContextualFragment(val);
          this.parentNode.replaceChild(el, this);
        }
        return MUX.$(id);
      }
    },

    dimensions: function() {
      var ds = this.getStyle('display');
      if (ds != 'none') {
        return {
          x: this.offsetWidth,
          y: this.offsetHeight
        };
      }
      // We have to temporary make the element displayed, but with a non/visibility
      // to make sure the dimensions comes out accurately.
      // Logic here inspired from prototype.js
      var st = this.style;
      var vis = st.visibility;
      var pos = st.position;
      var dis = st.display;
      st.visibility = 'hidden';
      st.display = 'block';
      var w = this.clientWidth;
      var h = this.clientHeight;
      st.display = dis;
      st.position = pos;
      st.visibility = vis;
      return {
        x: w,
        y: h
      };
    },

    getStyle: function(prop) {
      var val = this.style[prop];
      if (!val) {
        if (this.currentStyle) {
          val = this.currentStyle[prop];
        } else {
          var cs = document.defaultView.getComputedStyle(this, null);
          val = cs[prop];
        }
      }
      return val;
    },

    setStyle: function(prop, val) {
      this.style[prop] = val;
    },

    setStyles: function(vals) {
      for (var idx in vals) {
        this.setStyle(idx, vals[idx]);
      }
    },

    addClassName: function(val) {
      if (this.className.indexOf(val) == -1) {
        this.className += ' ' + val;
      }
    },

    // Removes a class name from the element
    removeClassName: function(val) {
      this.className =
      this.className.replace(
        new RegExp('(^|\\s+)' + val + '(\\s+|$)'), ' ')
          .replace(/^\s+/, '')
          .replace(/\s+$/, '');
    },

    toggleClassName: function(val) {
      if (this.className.indexOf(val) == -1) {
        this.addClassName(val);
      } else {
        this.removeClassName(val);
      }
    },

    setOpacity: function(op) {
      this.style.opacity =
      (op > 0.999 ?
        '' :
        (op < 0.001 ?
          0 :
          op));
    },

    getOpacity: function() {
      var op = this.getStyle('opacity');
      if (!op) {
        return 1.0;
      }
      return op;
    },

    // Helper for mouseenter/mouseleave support for non-IE based browsers...
    isLeaveEnter: function(e, node) {
      var rel =
      e.relatedTarget ?
        e.relatedTarget :
        (e.type == 'mouseout' ?
          e.toElement :
          e.fromElement);
      while (rel && rel != node) {
        rel = rel.parentNode;
      }
      return (rel != node);
    },

    observe: function(evt, func, ctx, pars) {
      if (!ctx) {
        ctx = this;
      }
      if (!this._funcWr) {
        this._funcWr = [];
      }
      if (!ctx._evtId) {
        ctx._evtId = MUX.Element._evtId++;
      }
      var T = this;
      this._funcWr[evt + ctx._evtId] =
      function(event) {
        var wrEvt = (event || window.event);
        if (evt == 'mouseover' || evt == 'mouseout') {
          if (!T.isLeaveEnter(wrEvt, T)) {
            return;
          }
        }
        if(ctx.element && ctx.element.disabled == 'disabled') {
          return;
        }
        var prs = (pars || []);
        prs.push(wrEvt);
        var retVal = func.apply(ctx, prs);
        if (retVal === false) {
          wrEvt.cancelBubble = true;
          wrEvt.returnValue = false;
          if (wrEvt.stopPropagation) {
            wrEvt.stopPropagation();
          }
          if (wrEvt.preventDefault) {
            wrEvt.preventDefault();
          }
          return false;
        }
      };
      if (this.addEventListener) {
        this.addEventListener(
        evt,
        this._funcWr[evt + ctx._evtId],
        false);
      } else {
        this.attachEvent(
        'on' + evt,
        this._funcWr[evt + ctx._evtId]);
      }
    },

    stopObserving: function(evt, func, ctx) {
      if (!ctx) {
        ctx = this;
      }
      if (this.removeEventListener) {
        this.removeEventListener(
        evt,
        this._funcWr[evt + ctx._evtId],
        false);
      } else {
        this.detachEvent(
        'on' + evt,
        this._funcWr[evt + ctx._evtId]);
      }
    }
  };


  /*
  * Class to help serialize HTML forms and create 
  * Ajax Request back to server with form values as POST request.
  */
  MUX.Form = MUX.klass();
  MUX.Form.beforeSerialization = [];

  MUX.Form.prototype = {

    _className: 'MUX.Form',

    init: function(form, opt) {
      // Defaulting to the first form on tha page...
      this.form = form || document.getElementsByTagName('form')[0];

      this.options = MUX.extend({
        args: '',
        onFinished: MUX.emptyFunction,
        onError: MUX.emptyFunction,
        callingContext: this,
        autoCallback: false
      }, opt || {});
      if (this.options.autoCallback) {
        this.callback(this.options.onFinished, this.options.onError);
      }
    },

    callback: function(onSuccess, onError) {
      var body = this.serializeForm();
      if (this.options.args) {
        body += '&' + this.options.args;
      }
      var xhr = new MUX.XHR(
        this.form.action, {
          body: body,
          onSuccess: function(response) {
            (onSuccess || this.options.onFinished).apply(this.options.callingContext, [response]);
          },
          onError: function(status, response) {
            (onError || this.options.onError).apply(this.options.callingContext, [status, response]);
          },
          callingContext: this
        });
    },

    serializeForm: function() {
      var bs = MUX.Form.beforeSerialization;
      var i = bs.length;
      while (i--) {
        bs[i].handler.apply(bs[i].context, []);
      }
      var val = [];
      var els = this.form.getElementsByTagName('*');
      var i = els.length;
      while (i--) {
        var el = els[i];
        if (el.name && !el.disabled) {
          switch (el.tagName.toLowerCase()) {
            case 'input':
            case 'textarea':
              var go = false;
              if (el.tagName.toLowerCase() == 'textarea') {
                go = true;
              } else {
                switch (el.type) {
                  case 'submit':
                    break;
                  case 'checkbox':
                  case 'radio':
                    if (el.checked) {
                      go = true;
                    }
                    break;
                  default: // We might have a TON of HTML5 element types here ...
                    go = true;
                    break;
                }
              }
              if (go) {
                val.push(el.name + '=' + encodeURIComponent(el.value));
              }
              break;
            case 'select':
              var ix = el.options.length;
              while (ix--) {
                if (el.options[ix].selected) {
                  val.push(el.name + '=' + encodeURIComponent(el.options[ix].value))
                }
              }
              break;
          }
        }
      }
      return val.join('&');
    }
  };


  /*
  * Ajax class. The high level class used 
  * to actually initiate callbacks to server.
  * This class will queue up all requests and only
  * allow one at the time to go to the server.
  */
  MUX.Ajax = MUX.klass();
  MUX.Ajax._requests = [];
  MUX.Ajax._pageUnloads = false;

  MUX.Ajax._loop = function() {
    if (MUX.XHR._hasRequest === false) {
      MUX.Ajax._requests[0].start();
    } else {
      setTimeout(function() {
        MUX.Ajax._loop();
      }, 50);
    }
  };

  MUX.Ajax.prototype = {

    _className: 'MUX.Ajax',

    init: function(opt) {
      if (MUX.Ajax._pageUnloads) {
        return;
      }
      this.options = MUX.extend({
        args: '',
        form: null, // Defaults to first form on page
        onBefore: MUX.emptyFunction,
        onSuccess: MUX.emptyFunction,
        onError: MUX.emptyFunction,
        callingContext: this
      }, opt || {});
      MUX.Ajax._requests.push(this);
      if (!MUX.XHR._hasRequest) {
        this.start();
      } else {
        MUX.Ajax._loop();
      }
    },

    start: function() {
      this.options.onBefore.apply(this.options.callingContext, []);
      var args = this.options.args;
      if (args && args.length > 0) {
        args += '&__RA_CALLBACK=true';
      }
      var form = new MUX.Form(this.options.form, {
        args: args,
        callingContext: this,
        onFinished: function(response) {
          MUX.Ajax._requests = MUX.Ajax._requests.slice(1);
          this.options.onSuccess.apply(this.options.callingContext, [response]);
        },
        onError: function(status, fullTrace) {
          MUX.Ajax._requests = MUX.Ajax._requests.slice(1);
          this.options.onError.apply(this.options.callingContext, [status, fullTrace]);
        },
        autoCallback: true
      });
    }
  };
})();
