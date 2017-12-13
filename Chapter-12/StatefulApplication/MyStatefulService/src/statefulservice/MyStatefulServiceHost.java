package statefulservice;

import java.time.Duration;
import java.util.logging.Logger;
import java.util.logging.Level;

import microsoft.servicefabric.services.runtime.ServiceRuntime;

public class MyStatefulServiceHost {

    private static final Logger logger = Logger.getLogger(MyStatefulServiceHost.class.getName());

    public static void main(String[] args) throws Exception{
        try {
            ServiceRuntime.registerStatefulServiceAsync("MyStatefulServiceType", (context)-> new MyStatefulService(context), Duration.ofSeconds(10));
            logger.log(Level.INFO, "Registered stateful service of type MyStatefulServiceType");
            Thread.sleep(Long.MAX_VALUE);
        } catch (Exception ex) {
            logger.log(Level.SEVERE, "Exception occurred", ex);
            throw ex;
        }
    }
}
