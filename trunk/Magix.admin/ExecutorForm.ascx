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
		PlaceHolder="Input Nodes ..."
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
			placeholder="Active Event ..."
			class="span7 monospaced"
			data-provide="typeahead" 
			data-items="10" 
			data-source='<%#GetDataSource()%>' />
		<mux:Button
			runat="server"
			id="run"
			Text="Execute"
			AccessKey="X"
			CssClass="span2 btn btn-primary"
			OnClick="run_Click" />
	</mux:Panel>
	<mux:TextArea
		runat="server"
		id="txtOut"
		Rows="8"
		PlaceHolder="Output Nodes ..."
		CssClass="span9 input-block-level monospaced" />
</div>