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
		style="opacity:0;"
		CssClass="message-box"
		id="messageWrapper">
		<br />
		<mux:Image
			runat="server"
			id="icon"
			CssClass="span-1 right"
			Visible="false" />
		<mux:Label
			runat="server"
			id="msgBoxHeader"
			Tag="label"
			CssClass="msg-box-header"
			Text="Message" />
		<mux:Label
			Tag="div"
			runat="server"
			id="messageLabel" />
	</mux:Panel>
    <mux:DynamicPanel 
        runat="server" 
        OnReload="dynamic_LoadControls"
        CssClass="span-24 last"
        id="content1" />
    <mux:DynamicPanel 
        runat="server" 
        OnReload="dynamic_LoadControls"
        CssClass="span-24 last"
        id="content2" />
    <mux:DynamicPanel 
        runat="server" 
        OnReload="dynamic_LoadControls"
        CssClass="span-24 last"
        id="content3" />
</mux:Panel>
