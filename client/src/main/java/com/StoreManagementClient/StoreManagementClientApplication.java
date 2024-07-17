package com.StoreManagementClient;

import org.springframework.boot.CommandLineRunner;
import org.springframework.boot.SpringApplication;
import org.springframework.boot.autoconfigure.SpringBootApplication;
import org.springframework.context.annotation.Bean;
import org.springframework.web.client.RestTemplate;

@SpringBootApplication
public class StoreManagementClientApplication {
    @Bean
    public RestTemplate restTemplate() {
        return new RestTemplate();
    }

    @Bean
    public CommandLineRunner welcome() {
        return args -> {
            System.out.println("Welcome to Store Management Client");
            System.out.println("Client is running on http://localhost:4321/Home");
        };
    }

    public static void main(String[] args) {
        SpringApplication.run(StoreManagementClientApplication.class, args);
    }
}
