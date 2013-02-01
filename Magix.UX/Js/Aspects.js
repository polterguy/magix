/*
* Magix - A Managed Ajax Library for ASP.NET
* Copyright 2010 - 2012 - QueenOfSpades20122@gmail.com
* Magix is licensed as MITx11, see enclosed License.txt File for Details.
*/

(function() {

  MUX.Aspect = MUX.klass();

  // Retrieve function
  MUX.Aspect.$ = function(id) {
    var ctrls = MUX.Control._ctrls;
    var idxCtrl = ctrls.length;
    while (idxCtrl--) {
      var behvs = ctrls[idxCtrl].options.aspects;
      var idxBeha = behvs.length;
      while (idxBeha--) {
        if (behvs[idxBeha].id == id) {
          return behvs[idxBeha];
        }
      }
    }
  };


  MUX.Aspect.prototype = {
    init: function(id, opt) {
      this.options = opt;
      this.id = id;
    },

    initBehavior: function(parent) {
      throw "Abstract base class [MUX.Aspect] used directly from JS. Something is really wrong...";
    },

    initBehaviorBase: function(parent) {
      this.parent = parent;
    },

    JSON: function(json) {
      for (var idxKey in json) {
        this[idxKey](json[idxKey]);
      }
    }
  };
})();
