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
		CssClass="span9 input-block-level" />
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
			class="span7"
			data-provide="typeahead" 
			data-items="20" 
			data-source='<%#GetDataSource()%>' />
		<mux:Button
			runat="server"
			id="run"
			Text="Execute"
			AccessKey="X"
			Tooltip="Runs the Active Event with the JSON Serialized content from Left Text Area"
			CssClass="span2 btn btn-primary"
			OnClick="run_Click" />
	</mux:Panel>
	<mux:TextArea
		runat="server"
		id="txtOut"
		Rows="8"
		PlaceHolder="Output Nodes ..."
		CssClass="span9 input-block-level" />
</div>