<%@ Assembly 
    Name="Magix.Core.Viewports" %>

<%@ Control 
    Language="C#" 
    AutoEventWireup="true" 
    Inherits="Magix.Core.Viewports.Website" %>

<mux:Panel
	runat="server"
	CssClass="message-wrapper"
	style="opacity:0;"
	id="messageWrapper">
	<mux:Label
		Tag="div"
		runat="server"
		id="messageLabel" />
</mux:Panel>

<mux:Panel
    runat="server"
    id="wrp"
    CssClass="main">
    <mux:DynamicPanel 
        runat="server" 
        OnReload="dynamic_LoadControls"
        id="content1" />
    <mux:DynamicPanel 
        runat="server" 
        OnReload="dynamic_LoadControls"
        id="content2" />
    <mux:DynamicPanel 
        runat="server" 
        OnReload="dynamic_LoadControls"
        id="content3" />
</mux:Panel>
