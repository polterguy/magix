<%@ Assembly 
    Name="Magix.viewports" %>

<%@ Control 
    Language="C#" 
    AutoEventWireup="true" 
    Inherits="Magix.viewports.SingleContainer" %>

<div class="container">
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
				Class="info-message-ok large span-3"
				Value="OK"
                OnEsc="CancelClick"
				OnClick="OKClick" />
			<mux:Button
				runat="server"
				id="cancel"
				Class="info-message-cancel large span-3 last"
				Value="Cancel"
                AccessKey="C"
				OnClick="CancelClick" />
		</div>
	</mux:Panel>
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
