<%@ Assembly 
    Name="Magix.viewports" %>

<%@ Control 
    Language="C#" 
    AutoEventWireup="true" 
    Inherits="Magix.viewports.Website" %>

<mux:Panel
	runat="server"
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
    CssClass="container showgrid">
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
