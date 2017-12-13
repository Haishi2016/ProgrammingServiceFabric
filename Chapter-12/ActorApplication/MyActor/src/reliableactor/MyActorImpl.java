package reliableactor;

import java.time.Duration;
import java.util.concurrent.CompletableFuture;
import java.util.logging.Level;
import java.util.logging.Logger;

import microsoft.servicefabric.actors.ActorServiceAttribute;
import microsoft.servicefabric.actors.FabricActor;
import microsoft.servicefabric.actors.StatePersistence;
import microsoft.servicefabric.actors.StatePersistenceAttribute;
import microsoft.servicefabric.actors.ActorId;
import microsoft.servicefabric.actors.FabricActorService;

/* 
This class represents an actor.
Every ActorID maps to an instance of this class.
The StatePersistence attribute determines persistence and replication of actor state:
  - Persisted: State is written to disk and replicated.
  - Volatile: State is kept in memory only and replicated.
  - None: State is kept in memory only and not replicated.
*/
@ActorServiceAttribute(name = "MyActorActorService")
@StatePersistenceAttribute(statePersistence = StatePersistence.Persisted)
public class MyActorImpl extends FabricActor implements MyActor {
    private Logger logger = Logger.getLogger(this.getClass().getName());

    public MyActorImpl(FabricActorService actorService, ActorId actorId){
        super(actorService, actorId);
    } 

    /*
    This method is called whenever an actor is activated.
    An actor is activated the first time any of its methods are invoked.
    */
    @Override
    protected CompletableFuture<?> onActivateAsync() {
        logger.log(Level.INFO, "onActivateAsync");

        /*
        The StateManager is this actor's private state store.
        Data stored in the StateManager will be replicated for high-availability for actors that use volatile or persisted state storage.
        Any serializable object can be saved in the StateManager.
        For more information, see http://aka.ms/servicefabricactorsstateserialization
        */

        return this.stateManager().tryAddStateAsync("count", 0);
    }

    // TODO: Replace with your own actor method.
    @Override
    public CompletableFuture<Integer> getCountAsync() {
        logger.log(Level.INFO, "Getting current count value");
        return this.stateManager().getStateAsync("count");
    }

    // TODO: Replace with your own actor method.
    @Override
    public CompletableFuture<?> setCountAsync(int count) {
        logger.log(Level.INFO, "Setting current count value {0}", count);
        return this.stateManager().addOrUpdateStateAsync("count", count, (key, value) -> count > value ? count : value);
    }

}
