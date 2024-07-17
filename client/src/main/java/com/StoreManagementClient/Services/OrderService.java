package com.StoreManagementClient.Services;

import com.StoreManagementClient.Middlewares.Converter;
import com.StoreManagementClient.Models.Order;
import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.beans.factory.annotation.Value;
import org.springframework.core.ParameterizedTypeReference;
import org.springframework.http.*;
import org.springframework.stereotype.Service;
import org.springframework.web.client.HttpClientErrorException;
import org.springframework.web.client.RestTemplate;

import java.util.List;
import java.util.Map;

@Service
public class OrderService {
    private final String baseUrl;
    private final RestTemplate restTemplate;

    @Autowired
    public OrderService(@Value("${api.base.url}") String baseUrl, RestTemplate restTemplate) {
        this.baseUrl = baseUrl + "/order";
        this.restTemplate = restTemplate;
    }

    public List<Order> getOrders() {
        // TODO: Implement getOrders
        return null;
    }

    public List<Order> getOrderByCustomerId(String id) {
        String url = baseUrl + "?customerId=" + id;

        HttpHeaders headers = new HttpHeaders();
        headers.setContentType(MediaType.APPLICATION_JSON);

        HttpEntity<Map<String, String>> requestEntity = new HttpEntity<>(headers);
        try {
            ResponseEntity<Map<String, Object>> apiResponse = restTemplate.exchange(
                    url,
                    HttpMethod.GET,
                    requestEntity,
                    new ParameterizedTypeReference<Map<String, Object>>() {
                    }
            );

            List<Order> orders = null;
            Map<String, Object> responseBody = apiResponse.getBody();
            if (responseBody != null && responseBody.containsKey("data"))
                orders = Converter.convertToOrders((List<Map<String, Object>>) responseBody.get("data"));

            return orders;
        } catch (HttpClientErrorException e) {
            return null;
        }

    }

    public Order getOrderById(String id) {
        // TODO: Implement getOrderById
        return null;
    }

}
