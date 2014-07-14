<%@ Assembly 
    Name="Magix.viewports" %>

<%@ Control 
    Language="C#" 
    AutoEventWireup="true" 
    Inherits="Magix.viewports.Gutenberg" %>

<mux:Panel
    runat="server"
	Class="container"
    id="container">
    <mux:Label
		runat="server"
		id="messageSmall"
		style="display:none;"
		Class="info-message-small" />
    <mux:Label
		runat="server"
		id="message"
		style="display:none;"
		Class="info-message" />
    <mux:Panel
		runat="server"
		id="confirmWrp"
		style="display:none;"
		OnEscKey="confirmWrp_Esc"
		Class="info-message">
		<mux:Label
			runat="server"
			Tag="p"
			id="confirmLbl" />
		<div class="confirm-button-wrapper span-6">
			<mux:Button
				runat="server"
				id="ok"
				Class="info-message-ok large span-3 bottom-1"
				Value="OK"
                OnEsc="CancelClick"
				OnClick="OKClick" />
			<mux:Button
				runat="server"
				id="cancel"
				Class="info-message-cancel large span-3 last bottom-1"
				Value="Cancel"
                AccessKey="C"
				OnClick="CancelClick" />
		</div>
	</mux:Panel>
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
        id="content5" />
    <mux:DynamicPanel 
        runat="server" 
        OnReload="dynamic_LoadControls"
        id="content6" />
    <mux:DynamicPanel 
        runat="server" 
        OnReload="dynamic_LoadControls"
        id="content7" />
    <mux:DynamicPanel 
        runat="server" 
        OnReload="dynamic_LoadControls"
        id="help" />
    <mux:DynamicPanel 
        runat="server" 
        OnReload="dynamic_LoadControls"
        id="footer" />
    <mux:DynamicPanel 
        runat="server" 
        OnReload="dynamic_LoadControls"
        id="trace" />
</mux:Panel>
