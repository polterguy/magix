<%@ Assembly 
    Name="Magix.viewports" %>

<%@ Control 
    Language="C#" 
    AutoEventWireup="true" 
    Inherits="Magix.viewports.Website" %>

<mux:Panel
    runat="server"
    id="wrp"
    CssClass="container">
	<mux:Panel
		runat="server"
		style="display:none;z-index:10000"
		CssClass="modal"
		id="messageWrapper">
		<mux:Image
			runat="server"
			id="icon"
			Visible="false" />
		<mux:Label
			runat="server"
			id="msgBoxHeader"
			Tag="h3"
			CssClass="modal-header"
			Text="Message from System" />
		<mux:Label
			Tag="div"
			runat="server"
			CssClass="modal-body"
			id="messageLabel" />
		<mux:Panel
			runat="server"
			id="modalFooter"
			Visible="false"
			CssClass="modal-footer">
			<mux:Button
				runat="server"
				id="modalClose"
				CssClass="btn btn-primary"
				OnClick="modalClose_Click"
				Text="close"/>
		</mux:Panel>
	</mux:Panel>

	<mux:Panel
		runat="server"
		id="backdrop"
		Visible="false"
		OnClick="CloseModal"
        CssClass="modal-backdrop" />
	<mux:Panel
		runat="server"
		id="mdlWrp"
		Visible="false"
        CssClass="modal">
        <div class="modal-header">
	        <mux:LinkButton 
	        	runat="server"
	        	Text="X"
	        	OnClick="CloseModal"
	        	CssClass="close"
	        	id="mdlClose" />
	        <mux:Label 
	        	runat="server"
	        	id="mdlHeader"
	        	Tag="h3" />
	    </div>
	    <mux:DynamicPanel 
	        runat="server" 
	        OnReload="dynamic_LoadControls"
	        CssClass="modal-body"
	        id="modal" />
	    <mux:DynamicPanel 
	        runat="server" 
	        OnReload="dynamic_LoadControls"
	        CssClass="modal-footer"
	        id="modalFtr" />
    </mux:Panel>

	<div class="row">
	    <mux:DynamicPanel 
	        runat="server" 
	        OnReload="dynamic_LoadControls"
	        CssClass="span12 text-center"
	        id="header" />
    </div>
	<div class="row">
	    <mux:DynamicPanel 
	        runat="server" 
	        OnReload="dynamic_LoadControls"
	        CssClass="span3"
	        id="menu" />
	    <mux:DynamicPanel 
	        runat="server" 
	        OnReload="dynamic_LoadControls"
	        CssClass="span9"
	        id="content" />
    </div>
</mux:Panel>
