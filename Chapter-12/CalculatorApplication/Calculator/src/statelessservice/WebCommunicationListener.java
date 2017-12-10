package statelessservice;

import java.util.concurrent.CompletableFuture;
import java.io.IOException;

import microsoft.servicefabric.services.communication.runtime.CommunicationListener;
import microsoft.servicefabric.services.runtime.StatelessServiceContext;
import system.fabric.description.EndpointResourceDescription;
import system.fabric.CancellationToken;


public class WebCommunicationListener implements CommunicationListener {
    private StatelessServiceContext context;
    private CalculatorServer server;
    private String webEndpointName = "ServiceEndpoint";
    private int port;
    public WebCommunicationListener(StatelessServiceContext context) {
    	this.context = context;
    	EndpointResourceDescription endpoint = this.context.getCodePackageActivationContext().getEndpoint(webEndpointName);
        this.port = endpoint.getPort();	
    }
    
    @Override
    public CompletableFuture<String> openAsync(CancellationToken cancellationTokan) {
    	CompletableFuture<String> str = new CompletableFuture<>();
    	String address = String.format("http://%s:%d/api", this.context.getNodeContext().getIpAddressOrFQDN(), this.port);
    	str.complete(address);
    	try
    	{
    	    server = new CalculatorServer(port);
    	    server.start();
    	} catch (IOException e) {
    		throw new RuntimeException(e);
    	}
    	return str;
    }
    
    @Override
    public CompletableFuture<?> closeAsync(CancellationToken cancellationToken) {
    	CompletableFuture<Boolean> task = new CompletableFuture<>();
    	task.complete(Boolean.TRUE);
    	if (server != null) {
    		server.stop();
    	}
    	return task;
    }
    
    @Override
    public void abort() {
    	if (server != null) {
    		server.stop();
    	}
    }
}
