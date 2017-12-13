package reliableactor;

import java.time.Duration;
import java.util.logging.Logger;
import java.util.logging.Level;

import microsoft.servicefabric.actors.ActorRuntime;
import microsoft.servicefabric.actors.FabricActorService;

public class MyActorActorHost {
    
    private static final Logger logger = Logger.getLogger(MyActorActorHost.class.getName());
    /*
    This is the entry point of the service host process.
    */
    public static void main(String[] args) throws Exception {
        
        try {
            
            /*
            This line registers an Actor Service to host your actor class with the Service Fabric runtime.
            For more information, see http://aka.ms/servicefabricactorsplatform
            */
            ActorRuntime.registerActorAsync(MyActorImpl.class, (context, actorType) -> new FabricActorService(context, actorType, (a,b)-> new MyActorImpl(a,b)), Duration.ofMinutes(10));
            Thread.sleep(Long.MAX_VALUE);
        } catch (Exception e) {
            logger.log(Level.SEVERE, "Exception occurred", e);
            throw e;
        }
    }
}

