function FindProxyForURL(url, host)
    {
        if (isPlainHostName(host) ||
	    dnsDomainIs(host, "mydomain.com")||
	    dnsDomainIs(host, "transit.nyct.com")||
	    dnsDomainIs(host, "mtaent.org") ||
	    dnsDomainIs(host, "mtabsc.info") ||
	    dnsDomainIs(host, "mta-bsc.org") ||
	    dnsDomainIs(host, "prodar.com")||
	    dnsDomainIs(host, "arprd.com")||
	    dnsDomainIs(host, "dev02.com")||
	    dnsDomainIs(host, "dev03.com")||
	    dnsDomainIs(host, "dev04.com")||
	    dnsDomainIs(host, "dev05.com")||
	    dnsDomainIs(host, "spock")||
        isInNet(host, "10.0.0.0", "255.0.0.0") ||
        isInNet(host, "65.51.184.72", "255.255.255.255") ||
        isInNet(host, "170.28.128.128", "255.255.255.240") ||
 	    isInNet(host, "170.28.128.44", "255.255.255.0") ||
	    isInNet(host, "170.28.136.252", "255.255.255.255") ||
	    isInNet(host, "170.28.7.200", "255.255.255.255") ||
	    isInNet(host, "170.28.7.201", "255.255.255.255") ||
	    isInNet(host, "88.14.1.0", "255.255.255.0") ||
	    isInNet(host, "127.0.0.0", "255.0.0.0") ||
	    isInNet(host, "129.225.118.171", "255.255.255.255") ||
	    isInNet(host, "129.225.192.10", "255.255.255.255") ||
	    isInNet(host, "172.16.0.0", "255.240.0.0") ||
	    isInNet(host, "172.23.0.0", "255.240.0.0") ||
	    isInNet(host, "172.25.0.0", "255.255.0.0") ||
	    isInNet(host, "172.25.25.13", "255.255.0.0") ||
    	isInNet(host, "192.168.0.0", "255.255.0.0") ||
	    isInNet(host, "159.181.33.0", "255.255.255.0") ||
	    isInNet(host, "159.181.56.0", "255.255.255.0") ||
	    isInNet(host, "160.79.24.20", "255.255.255.0") ||
	    isInNet(host, "160.79.24.21", "255.255.255.0") ||
	    isInNet(host, "160.79.24.22", "255.255.255.0") ||
	    isInNet(host, "161.185.121.0", "255.255.255.0") ||
	    isInNet(host, "161.185.185.0", "255.255.255.0") ||
	    isInNet(host, "161.185.249.0", "255.255.255.0") ||
	    isInNet(host, "192.61.1.18", "255.255.255.255") ||
	    isInNet(host, "192.61.1.4", "255.255.255.255") ||
	    isInNet(host, "192.61.1.23", "255.255.255.255") ||
    	isInNet(host, "192.61.1.22", "255.255.255.255") ||
    	isInNet(host, "192.61.59.121", "255.255.255.255") ||
    	isInNet(host, "192.61.59.65", "255.255.255.255") ||
    	isInNet(host, "192.61.177.37", "255.255.255.255") ||
    	isInNet(host, "192.61.177.38", "255.255.255.255") ||
	    isInNet(host, "192.61.200.101", "255.255.255.255") ||
    	isInNet(host, "64.238.200.200", "255.255.255.255"))
           return "DIRECT";
        else
           
   switch (parseInt((myIpAddress().split("."))[3],10) % 2)
        { 
        // // even numbers are case 0
        case 0 : return "PROXY 10.52.11.48:8080; PROXY 10.52.11.49:8080; PROXY 10.9.91.48:8080; PROXY 10.9.95.48:8080"; 
        // // odd numbers are case 1
        case 1 : return "PROXY 10.52.11.49:8080; PROXY 10.52.11.48:8080; PROXY 10.9.95.48:8080; PROXY 10.9.91.48:8080";
		}

    }
