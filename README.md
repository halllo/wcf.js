wcf.js
======
Experimenting invoking WCF services with JavaScript. The WCF endpoints use WSHttpBinding which correspond to SOAP1.2-Envelopes (http://www.w3.org/2003/05/soap-envelope).

Can my WCF backend acknowledge the CORS preflight? WCF cannot:
![Screenshot](https://raw.github.com/halllo/wcf.js/master/wcfwithoutcors.png)

Hosting a CORS middleware with OWIN at the same origin does not solve the problem. The entire web app needs to be hosted at the WCF server origin because then the browser does not emit the CORS preflight request.