package com.StoreManagementClient.Services;

import com.StoreManagementClient.Middlewares.Converter;
import com.StoreManagementClient.Models.Product;
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
public class ProductService {
    private final String baseUrl;
    private final RestTemplate restTemplate;

    @Autowired
    public ProductService(@Value("${api.base.url}") String baseUrl, RestTemplate restTemplate) {
        this.baseUrl = baseUrl + "/product";
        this.restTemplate = restTemplate;
    }

    public List<Product> getProducts(String text) {
        String url = baseUrl;
        if (text != null && !text.isEmpty()) url += "?text=" + text;

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

            List<Product> products = null;
            Map<String, Object> responseBody = apiResponse.getBody();
            if (responseBody != null && responseBody.containsKey("data"))
                products = Converter.convertToProducts((List<Map<String, Object>>) responseBody.get("data"));

            return products;
        } catch (HttpClientErrorException e) {
            return null;
        }
    }

    public Object createProduct(Product product) {
        String url = baseUrl + "/create";

        HttpHeaders headers = new HttpHeaders();
        headers.setContentType(MediaType.APPLICATION_JSON);

        HttpEntity<Product> requestEntity = new HttpEntity<>(product, headers);
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
            System.out.println(e);
            Map<String, Object> responseBody = e.getResponseBodyAs(Map.class);
            if (responseBody == null || !responseBody.containsKey("message"))
                throw new HttpServerErrorException(HttpStatus.INTERNAL_SERVER_ERROR, "Empty response body");

            return responseBody.get("message");
        }
    }

    public Object updateProduct(Product product) {
        String url = baseUrl + "/update/" + product.getPid();

        HttpHeaders headers = new HttpHeaders();
        headers.setContentType(MediaType.APPLICATION_JSON);

        HttpEntity<Product> requestEntity = new HttpEntity<>(product, headers);
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

    public Object deleteProduct(String id) {
        String url = baseUrl + "/delete/" + id;

        HttpHeaders headers = new HttpHeaders();
        headers.setContentType(MediaType.APPLICATION_JSON);

        HttpEntity<Map<String, String>> requestEntity = new HttpEntity<>(headers);
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
            return Converter.convertToProducts((List<Map<String, Object>>) responseBody.get("data"));
        else if (responseBody.containsKey("message"))
            return responseBody.get("message");
        else
            throw new HttpServerErrorException(HttpStatus.INTERNAL_SERVER_ERROR, "Empty response body");
    }
}
