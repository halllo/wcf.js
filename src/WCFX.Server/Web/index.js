(function () {
	"use strict";

	$(document).ready(function () {
		$('#requestServer').click(requestServer);
	});


	function requestServer() {
		try {

			var fullAccessRights = $('#runWithFullAccessRights').is(":checked");
			var url = 'https://localhost:8001/WCFX/WCFX.Common.IDossierService';
			var request = new XMLHttpRequest();
			request.open('POST', url, true);
			request.setRequestHeader("Content-type", "application/soap+xml; charset=utf-8");
			request.onreadystatechange = function () {
				if (request.readyState == 4 && request.status == 200) {
					
					var matches = request.responseText.match(/<b:ReferenceNumber>(.*?)<\/b:ReferenceNumber>/g).map(function(value) {
						return value.replace("<b:ReferenceNumber>", "").replace("<\/b:ReferenceNumber>", "");
					});
					showNotification("result: " + matches.length, matches);
				}
			}
			request.send(
'<soap:Envelope xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance" xmlns:xsd="http://www.w3.org/2001/XMLSchema" xmlns:soap="http://www.w3.org/2003/05/soap-envelope">\
<soap:Header xmlns:wsa="http://www.w3.org/2005/08/addressing">\
<wsa:To>https://localhost:8001/WCFX/WCFX.Common.IDossierService</wsa:To>\
<wsa:Action>http://tempuri.org/IDossierService/GetAll</wsa:Action>\
</soap:Header>\
<soap:Body>\
<GetAll xmlns="http://tempuri.org/">\
<runWithFullAccessRights>' + fullAccessRights + '</runWithFullAccessRights>\
</GetAll>\
</soap:Body>\
</soap:Envelope>'
			);

		}
		catch (err) {
			showNotification("error", err.message || err);
		}
	}

























	function showNotification(header, text) {
		$('#notification-message-header').text(header);
		$('#notification-message-body').text(text);
	};

})();