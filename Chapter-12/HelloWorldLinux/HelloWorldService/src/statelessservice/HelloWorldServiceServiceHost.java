package statelessservice;

import java.time.Duration;
import java.util.logging.Logger;
import java.util.logging.Level;
import java.util.logging.FileHandler;
import java.util.logging.SimpleFormatter;

import microsoft.servicefabric.services.runtime.ServiceRuntime;

public class HelloWorldServiceServiceHost {

    private static final Logger logger = Logger.getLogger(HelloWorldServiceServiceHost.class.getName());

    public static void main(String[] args) throws Exception{
    	FileHandler handler = new FileHandler("myapp-log.%u.%g.txt", 1024000, 10, true);
    	handler.setFormatter(new SimpleFormatter());
    	handler.setLevel(Level.ALL);
    	logger.addHandler(handler);    	
        try {
            ServiceRuntime.registerStatelessServiceAsync("HelloWorldServiceType", (context)-> new HelloWorldServiceService(), Duration.ofSeconds(10));
            logger.log(Level.INFO, "Registered stateless service of type HelloWorldServiceType");
            Thread.sleep(Long.MAX_VALUE);
        } catch (Exception ex) {
            logger.log(Level.SEVERE, "Exception occurred", ex);
            throw ex;
        }
    }
}
