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
		style="opacity:0;position:fixed;top:0;left:0;width:100%;z-index:10000;"
		CssClass="alert"
		id="messageWrapper">
		<mux:Image
			runat="server"
			id="icon"
			Visible="false" />
		<mux:Label
			runat="server"
			id="msgBoxHeader"
			Tag="label"
			Text="Message" />
		<mux:Label
			Tag="div"
			runat="server"
			id="messageLabel" />
	</mux:Panel>
	<div class="row">
	    <mux:DynamicPanel 
	        runat="server" 
	        OnReload="dynamic_LoadControls"
	        CssClass="span7 offset5"
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
