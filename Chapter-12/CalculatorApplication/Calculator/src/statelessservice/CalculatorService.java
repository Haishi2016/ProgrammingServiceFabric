package statelessservice;

import java.util.concurrent.CompletableFuture;
import java.util.List;
import java.util.ArrayList;

import system.fabric.CancellationToken;

import microsoft.servicefabric.services.communication.runtime.ServiceInstanceListener;
import microsoft.servicefabric.services.runtime.StatelessService;

public class CalculatorService extends StatelessService {

    @Override
    protected List<ServiceInstanceListener> createServiceInstanceListeners() {
        ArrayList<ServiceInstanceListener> listeners = new ArrayList();
        listeners.add(new ServiceInstanceListener((context) -> {
        	return new WebCommunicationListener(context);
        }));
        return listeners;
    }
}
