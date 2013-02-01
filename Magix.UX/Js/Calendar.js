/*
* Magix - A Managed Ajax Library for ASP.NET
* Copyright 2010 - 2012 - QueenOfSpades20122@gmail.com
* Magix is licensed as MITx11, see enclosed License.txt File for Details.
*/

(function() {

  MUX.Calendar = MUX.klass();

  MUX.extend(MUX.Calendar.prototype, MUX.Control.prototype);

  MUX.extend(MUX.Calendar.prototype, {
    init: function(el, opt) {
      this.initControl(el, opt);
      this.options = MUX.extend({
        prefix: ''
      }, this.options || {});
      this.initCalendar();
    },

    initCalendar: function() {
      this._cells = [];
      var tbl = MUX.$(this.options.prefix + 'tbl');
      var children = tbl.childNodes[0].childNodes;
      var idx = children.length;
      while (idx--) {
        var elTr = children[idx];
        var idxInner = elTr.childNodes.length;
        while (idxInner--) {
          var el = elTr.childNodes[idxInner];
          if (el.id != '') {
            // Date cell...
            this._cells.push({ el: MUX.$(el), T: this, cell: el });
          }
        }
      }
      idx = this._cells.length;
      while (idx--) {
        this._cells[idx].el.observe('click', this.dateClicked, this._cells[idx]);
      }
    },

    dateClicked: function() {
      var date = this.cell.id.replace(this.T.options.prefix, '');
      var x = new MUX.Ajax({
        args: '__MUX_CONTROL_CALLBACK=' + this.T.element.id + '&__MUX_EVENT=changeDate&__date=' + date,
        onSuccess: this.T.onFinishedRequest,
        callingContext: this.T
      });
    },

    reRender: function(html) {
      this._reRender(html);
      this.initCalendar();
    },

    destroyThis: function() {
      var idx = this._cells.length;
      while (idx--) {
        this._cells[idx].el.stopObserving('click', this.dateClicked, this._cells[idx]);
      }
      this._destroyThisControl();
    }
  });
})();
