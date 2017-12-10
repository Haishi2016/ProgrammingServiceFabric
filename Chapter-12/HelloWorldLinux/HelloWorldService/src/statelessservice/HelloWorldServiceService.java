package statelessservice;

import java.util.concurrent.CompletableFuture;
import java.util.List;

import system.fabric.CancellationToken;

import microsoft.servicefabric.services.communication.runtime.ServiceInstanceListener;
import microsoft.servicefabric.services.runtime.StatelessService;
import java.util.logging.Level;
import java.util.logging.Logger;
import java.util.logging.FileHandler;
import java.util.logging.SimpleFormatter;


public class HelloWorldServiceService extends StatelessService {
	private static final Logger logger = Logger.getLogger(HelloWorldServiceService.class.getName());

    @Override
    protected List<ServiceInstanceListener> createServiceInstanceListeners() {
        // TODO: If your service needs to handle user requests, return the list of ServiceInstanceListeners from here.
        return super.createServiceInstanceListeners();
    }

    @Override
    protected CompletableFuture<?> runAsync(CancellationToken cancellationToken) {
    	try
    	{
    	    String logPath = super.getServiceContext().getCodePackageActivationContext().getLogDirectory();
    	    FileHandler handler = new FileHandler(logPath + "/mysrv-log.%u.%g.txt", 1024000, 10, true);
    	    handler.setFormatter(new SimpleFormatter());
       	    handler.setLevel(Level.ALL);
    	    logger.addHandler(handler);    	
    	} catch (Exception exp) {
    	    logger.log(Level.SEVERE, null, exp);
    	}
    	return CompletableFuture.runAsync(() -> {
    		try
    		{
    	        int iteration = 0;
                while (!cancellationToken.isCancelled()) {
        	       logger.log(Level.INFO, "Working-" + iteration++);
        	       Thread.sleep(1000);
                }
    		} catch (Exception exp) {
    			logger.log(Level.SEVERE, null, exp);
    		}
    	});
    }
}
