<%@ Assembly 
    Name="Magix.admin" %>

<%@ Control 
    Language="C#" 
    AutoEventWireup="true" 
    Inherits="Magix.admin.ExecutorForm" %>

<div class="fill-width">
	<mux:TextArea
		runat="server"
		id="txtIn"
		Rows="15"
		PlaceHolder="input nodes ..."
		CssClass="fill-width monospaced" />
	<mux:Panel
		runat="server"
		ID="exeWrp"
		DefaultWidget="run"
		CssClass="btn-group span-19 last">
		<input
			id="activeEvent"
			runat="server"
			type="text"
			placeholder="active event ..."
			class="span-13 monospaced input-large"
			data-provide="typeahead" 
			data-items="10" 
			data-source='<%#GetDataSource()%>' />
		<mux:Button
			runat="server"
			id="run"
			Text="execute"
			AccessKey="X"
			CssClass="span-3 btn-large"
			OnClick="run_Click" />
		<mux:Button
			runat="server"
			id="move"
			Text="move up"
			AccessKey="U"
			CssClass="span-3 last btn-large"
			OnClick="move_Click" />
	</mux:Panel>
	<mux:TextArea
		runat="server"
		id="txtOut"
		Rows="15"
		PlaceHolder="output nodes ..."
		CssClass="fill-width" />
</div>