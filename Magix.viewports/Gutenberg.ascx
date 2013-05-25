<%@ Assembly 
    Name="Magix.viewports" %>

<%@ Control 
    Language="C#" 
    AutoEventWireup="true" 
    Inherits="Magix.viewports.Gutenberg" %>

<link href="media/grid/main.css" rel="stylesheet" media="screen" />

<mux:Panel
    runat="server"
	CssClass="container showgrid"
    id="wrp">
    <mux:Label
		runat="server"
		id="message"
		style="display:none;"
		CssClass="info-message" />
    <mux:DynamicPanel 
        runat="server" 
        OnReload="dynamic_LoadControls"
        id="header" />
    <mux:DynamicPanel 
        runat="server" 
        OnReload="dynamic_LoadControls"
        id="menu" />
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
    <mux:DynamicPanel 
        runat="server" 
        OnReload="dynamic_LoadControls"
        id="content4" />
    <mux:DynamicPanel 
        runat="server" 
        OnReload="dynamic_LoadControls"
        id="footer" />
    <mux:DynamicPanel 
        runat="server" 
        OnReload="dynamic_LoadControls"
        id="trace" />
</mux:Panel>
