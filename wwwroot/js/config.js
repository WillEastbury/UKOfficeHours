var authinstance = 'https://login.microsoftonline.com/'; // Azure AD logon endpoint
var dateform = "DD/MM/YYYY"; // UK locale
var timeform = "HH:mm"; // 12h UK time

// Figure out what the current URI is and where to get our config from in the main script
var namesplitter = "-ukofficehours";
var serverprefixaddress = window.location.href.split("/")[2].split(".")[0];
var prefix = serverprefixaddress.split(namesplitter)[0]
var resource = 'https://' + prefix + 'ukohfn.azurewebsites.net';
var endpoint = 'https://' + prefix + '-ukofficehours.azurewebsites.net/';
var rootfnsite = resource + "/";

console.info("Site Running at:" + endpoint);
console.info("Server Running at:" + rootfnsite);

// Pick up server specific settings and load the ad config via a function call from the remote server
var clientid = '';
var tenantid = '';

$.ajax({
    method: "GET",
    url: rootfnsite + "api/GetConfig",
    success: function(result) {
        // we have data, update the viewmodel and let knockout take care of the binding
        clientid = result.ClientId;
        tenantid = result.TenantId;
        console.info("Client ID is:" + clientid);
        console.info("Tenant ID is:" + tenantid);
    },
    error: function(request, status, error) {
        alert('Failed to load config:' + request.responseText);
        console.error('Failed to load config:' + request.responseText);
    }
});