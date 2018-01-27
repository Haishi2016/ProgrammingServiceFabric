package hello;

import org.springframework.boot.SpringApplication;
import org.springframework.boot.autoconfigure.SpringBootApplication;
import org.springframework.web.bind.annotation.RequestMapping;
import org.springframework.web.bind.annotation.RestController;

@SpringBootApplication
@RestController
public class Application {

    boolean enableFlag = true;
    @RequestMapping("/")
    public String home() {
        if(enableFlag)
            return "I am good";
        else
            return "something wrong";
    }

    @RequestMapping("/pause")
    public void pause(){
        this.enableFlag = false;
    }

    @RequestMapping("/resume")
    public void resume(){
        this.enableFlag = true;
    }

    public static void main(String[] args) {
        SpringApplication.run(Application.class, args);
    }

}
