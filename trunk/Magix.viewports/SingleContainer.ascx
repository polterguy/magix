<%@ Assembly 
    Name="Magix.viewports" %>

<%@ Control 
    Language="C#" 
    AutoEventWireup="true" 
    Inherits="Magix.viewports.SingleContainer" %>

<div class="container">
    <mux:Label
	    runat="server"
	    id="messageSmall"
	    style="display:none;"
	    Class="info-message-small" />
    <mux:DynamicPanel 
        runat="server" 
        OnReload="dynamic_LoadControls"
        id="content" />
</div>
