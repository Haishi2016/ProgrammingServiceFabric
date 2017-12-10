package statefulservice;

import java.time.Duration;
import java.util.ArrayList;
import java.util.List;
import java.util.concurrent.CompletableFuture;
import java.util.logging.Level;
import java.util.logging.Logger;

import microsoft.servicefabric.data.ReliableStateManager;
import microsoft.servicefabric.data.Transaction;
import microsoft.servicefabric.services.communication.runtime.ServiceReplicaListener;
import microsoft.servicefabric.services.runtime.StatefulService;
import system.fabric.CancellationToken;
import system.fabric.StatefulServiceContext;

public class StatefulService extends StatefulService {
    private ReliableStateManager stateManager;
    private static final Logger logger = Logger.getLogger(StatefulService.class.getName());

    protected StatefulService (StatefulServiceContext statefulServiceContext) {
        super (statefulServiceContext);
        this.stateManager = this.getReliableStateManager();
    }

    @Override
    protected List<ServiceReplicaListener> createServiceReplicaListeners() {
        // Create your own ServiceReplicaListeners and add to the listenerList.
        List<ServiceReplicaListener> listenerList = new ArrayList<>();
        // listenerList.add(listener1);
        return listenerList;
    }

    @Override
    protected CompletableFuture<?> runAsync(CancellationToken cancellationToken) {
    // TODO: Replace the following sample code with your own logic 
    // or remove this runAsync override if it's not needed in your service.
    
        Transaction tx = stateManager.createTransaction();
        return this.stateManager.<String, Long>getOrAddReliableHashMapAsync("myHashMap").thenCompose((map) -> {
            return map.computeAsync(tx, "counter", (k, v) -> {
                if (v == null)
                    return 1L;
                else
                    return ++v;
            }, Duration.ofSeconds(4), cancellationToken).thenApply((l) -> {
                return tx.commitAsync().handle((r, x) -> {
                    if (x != null) {
                        logger.log(Level.SEVERE, x.getMessage());
                    }
                    try {
                        tx.close();
                    } catch (Exception e) {
                        logger.log(Level.SEVERE, e.getMessage());
                    }
                    return null;
                });
            });
        });
    }
}
