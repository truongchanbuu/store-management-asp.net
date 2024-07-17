package com.StoreManagementClient.Services;

import com.StoreManagementClient.Middlewares.Converter;
import com.StoreManagementClient.Models.Customer;
import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.beans.factory.annotation.Value;
import org.springframework.core.ParameterizedTypeReference;
import org.springframework.http.*;
import org.springframework.stereotype.Service;
import org.springframework.web.client.HttpClientErrorException;
import org.springframework.web.client.HttpServerErrorException;
import org.springframework.web.client.RestTemplate;

import java.util.List;
import java.util.Map;

@Service
public class CustomerService {
    private final String baseUrl;
    private final RestTemplate restTemplate;

    @Autowired
    public CustomerService(@Value("${api.base.url}") String baseUrl, RestTemplate restTemplate) {
        this.baseUrl = baseUrl + "/customer";
        this.restTemplate = restTemplate;
    }

    public List<Customer> getUsers(String phone, String name) {
        String url = baseUrl + "?phone={phone}&name={name}";
        if (phone != null && !phone.isEmpty()) url = url.replace("{phone}", phone);
        else url = url.replace("{phone}", "");

        if (name != null && !name.isEmpty()) url = url.replace("{name}", name);
        else url = url.replace("{name}", "");

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

            List<Customer> customers = null;
            Map<String, Object> responseBody = apiResponse.getBody();
            if (responseBody != null && responseBody.containsKey("data"))
                customers = Converter.convertToCustomers((List<Map<String, Object>>) responseBody.get("data"));

            return customers;
        } catch (HttpClientErrorException e) {
            return null;
        }
    }

    public Customer getUserById(String id) {
        String url = baseUrl + "/" + id;

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

            Customer customer = null;
            Map<String, Object> responseBody = apiResponse.getBody();
            if (responseBody != null && responseBody.containsKey("data"))
                customer = Converter.convertToCustomers((List<Map<String, Object>>) responseBody.get("data")).get(0);

            return customer;
        } catch (HttpClientErrorException e) {
            return null;
        }
    }

    public Object updateCustomer(String id, Customer customer) {
        String url = baseUrl + "/update/" + id;

        HttpHeaders headers = new HttpHeaders();
        headers.setContentType(MediaType.APPLICATION_JSON);

        HttpEntity<Customer> requestEntity = new HttpEntity<>(customer, headers);
        try {
            ResponseEntity<Map<String, Object>> apiResponse = restTemplate.exchange(
                    url,
                    HttpMethod.POST,
                    requestEntity,
                    new ParameterizedTypeReference<Map<String, Object>>() {
                    }
            );

            return getObjectFromApiResponse(apiResponse);
        } catch (HttpClientErrorException e) {
            Map<String, Object> responseBody = e.getResponseBodyAs(Map.class);
            if (responseBody == null || !responseBody.containsKey("message"))
                throw new HttpServerErrorException(HttpStatus.INTERNAL_SERVER_ERROR, "Empty response body");

            return responseBody.get("message");
        }
    }

    private Object getObjectFromApiResponse(ResponseEntity<Map<String, Object>> apiResponse) {
        Map<String, Object> responseBody = apiResponse.getBody();
        if (responseBody == null)
            throw new HttpServerErrorException(HttpStatus.INTERNAL_SERVER_ERROR, "Empty response body");

        if (responseBody.containsKey("data"))
            return Converter.convertToCustomers((List<Map<String, Object>>) responseBody.get("data"));
        else if (responseBody.containsKey("message"))
            return responseBody.get("message");
        else
            throw new HttpServerErrorException(HttpStatus.INTERNAL_SERVER_ERROR, "Empty response body");
    }
}
