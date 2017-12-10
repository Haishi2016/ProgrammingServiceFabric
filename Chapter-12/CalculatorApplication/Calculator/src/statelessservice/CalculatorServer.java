package statelessservice;

import com.sun.net.httpserver.*;
import java.net.InetSocketAddress;
import java.io.IOException;
import java.io.OutputStream;
import java.io.UnsupportedEncodingException;
import java.util.HashMap;
import java.util.Map;


public class CalculatorServer {
	private HttpServer server;
    private int port;
    public CalculatorServer(int port) {
    	this.port = port;
    }
    public void start() throws IOException {
    	server = HttpServer.create(new InetSocketAddress(port),0);
    	HttpHandler add = new HttpHandler() {
    		@Override
    		public void handle(HttpExchange h) throws IOException {
    			byte[] buffer = CalculatorServer.handleCalculation(h.getRequestURI().getQuery(), "add");
    			h.sendResponseHeaders(200, buffer.length);
    			OutputStream os = h.getResponseBody();
    			os.write(buffer);
    			os.close();
    		}
    	};
    	HttpHandler subtract = new HttpHandler() {
    		@Override
    		public void handle(HttpExchange h) throws IOException {
    			byte[] buffer = CalculatorServer.handleCalculation(h.getRequestURI().getQuery(), "subtract");
    			h.sendResponseHeaders(200, buffer.length);
    			OutputStream os = h.getResponseBody();
    			os.write(buffer);
    			os.close();
    		}
    	};
    	server.createContext("/api/add", add);
    	server.createContext("/api/subtract", subtract);
    	server.setExecutor(null);
    	server.start();
    }
    public void stop() {
    	server.stop(10);
    }
    public static Map<String, String> queryToMap(String query) {
    	Map<String, String> map = new HashMap<String, String>();
    	for (String param: query.split("&")) {
    		String pair[] = param.split("=");
    		if (pair.length > 1) {
    			map.put(pair[0], pair[1]);
    		} else {
    			map.put(pair[0], "0");
    		}
    	}
    	return map;
    }
    public static byte[] handleCalculation(String query, String type) throws UnsupportedEncodingException {
    	byte[] buffer = null;
    	Map<String, String> parameters = CalculatorServer.queryToMap(query);
    	int c = 0;
    	try
    	{
    	    int a = Integer.parseInt(parameters.get("a"));
    	    int b = Integer.parseInt(parameters.get("b"));
    	    if (type.equals("add")) {
    		    c = a + b;
    	    } else {
    		    c = a - b;
    	    }
    	    buffer = Integer.toString(c).getBytes("UTF-8");
    	} catch (NumberFormatException e) {
    		buffer = ("Invalid parameters").getBytes("UTF-8");
    	}
    	
    	return buffer;
    }   
}
