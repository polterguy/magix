<%@ Assembly 
    Name="Magix.admin" %>

<%@ Control 
    Language="C#" 
    AutoEventWireup="true" 
    Inherits="Magix.admin.ExecutorForm" %>

<div class="span9">
	<mux:TextArea
		runat="server"
		id="txtIn"
		Rows="12"
		PlaceHolder="input nodes ..."
		CssClass="span9 input-block-level monospaced" />
	<mux:Panel
		runat="server"
		ID="exeWrp"
		DefaultWidget="run"
		CssClass="input-append span9">
		<input
			id="activeEvent"
			runat="server"
			type="text"
			placeholder="active event ..."
			class="span5 monospaced"
			data-provide="typeahead" 
			data-items="10" 
			data-source='<%#GetDataSource()%>' />
		<mux:Button
			runat="server"
			id="run"
			Text="execute"
			AccessKey="X"
			CssClass="span2 btn btn-primary"
			OnClick="run_Click" />
		<mux:Button
			runat="server"
			id="move"
			Text="move up"
			AccessKey="U"
			CssClass="span2 btn btn-primary"
			OnClick="move_Click" />
	</mux:Panel>
	<mux:TextArea
		runat="server"
		id="txtOut"
		Rows="8"
		PlaceHolder="output nodes ..."
		CssClass="span9 input-block-level monospaced" />
</div>