(function () {
	"use strict";

	var clientId = '...';
	var audience = '...';
	var samlAccessToken = '';

	var authContext = new AuthenticationContext({
		clientId: clientId
	});

	$(document).ready(function () {
		$('#signin').click(signin);
		$('#signout').click(signout);
		$('#requestServer').click(requestServer);

		if (authContext.isCallback(window.location.hash)) {
			authContext.handleWindowCallback();
			var err = authContext.getLoginError();
			if (err) {
				alert('ERROR:\n\n' + err);
			}
		} else {
			var user = authContext.getCachedUser();
			if (user) {
				console.info('Getting access token...');
				authContext.acquireToken(audience, function (error, token) {
					if (error || !token) {
						console.info('oh oh');
						alert('ERROR:\n\n' + error);
						return;
					}
					console.info('got it!');
					newToken(token);
					showNotification('Signed in as: ' + user.userName, 'SAML:\n' + samlAccessToken);
				});
			} else {
				showNotification('Not signed in.');
			}
		}
	});
	
	function signin() {
		authContext.login();
	};

	function signout() {
		authContext.logOut();
	};

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
'<s:Envelope xmlns:s="http://www.w3.org/2003/05/soap-envelope" xmlns:a="http://www.w3.org/2005/08/addressing" xmlns:u="http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-utility-1.0.xsd">\
  <s:Header>\
    <a:Action>http://tempuri.org/IDossierService/GetAll</a:Action>\
    <a:To>' + url + '</a:To>\
    <o:Security xmlns:o="http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-secext-1.0.xsd">\
      ' + samlAccessToken + '\
    </o:Security>\
  </s:Header>\
  <s:Body>\
    <GetAll xmlns="http://tempuri.org/">\
      <runWithFullAccessRights>' + fullAccessRights + '</runWithFullAccessRights>\
    </GetAll>\
  </s:Body>\
</s:Envelope>'
			);

		}
		catch (err) {
			showNotification("error", err.message || err);
		}
	};




	function newToken(jwt) {
		var assertionId = '_' + guid();
		var assertionIssued = new Date().toISOString();
		samlAccessToken =
'<Assertion ID="' + assertionId + '" IssueInstant="' + assertionIssued + '" Version="2.0" xmlns="urn:oasis:names:tc:SAML:2.0:assertion">\
  <Issuer>urn:wrappedjwt</Issuer>\
  <Subject>\
    <SubjectConfirmation Method="urn:oasis:names:tc:SAML:2.0:cm:bearer" />\
  </Subject>\
  <AttributeStatement>\
    <Attribute Name="jwt">\
      <AttributeValue>' + jwt + '</AttributeValue>\
    </Attribute>\
  </AttributeStatement>\
</Assertion>';
	};




















	function showNotification(header, text) {
		$('#notification-message-header').text(header);
		$('#notification-message-body').text(text);
	};

	function guid() {
		function s4() {
			return Math.floor((1 + Math.random()) * 0x10000)
				.toString(16)
				.substring(1);
		};
		return s4() + s4() + '-' + s4() + '-' + s4() + '-' + s4() + '-' + s4() + s4() + s4();
	};

})();