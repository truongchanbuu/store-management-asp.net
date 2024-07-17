package com.StoreManagementClient.Services;

import com.StoreManagementClient.Middlewares.Converter;
import com.StoreManagementClient.Models.User;
import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.beans.factory.annotation.Value;
import org.springframework.core.ParameterizedTypeReference;
import org.springframework.http.*;
import org.springframework.stereotype.Service;
import org.springframework.web.client.HttpClientErrorException;
import org.springframework.web.client.HttpServerErrorException;
import org.springframework.web.client.RestTemplate;

import java.util.HashMap;
import java.util.List;
import java.util.Map;

@Service
public class UserService {
    private final String baseUrl;
    private final RestTemplate restTemplate;

    @Autowired
    public UserService(@Value("${api.base.url}") String baseUrl, RestTemplate restTemplate) {
        this.baseUrl = baseUrl;
        this.restTemplate = restTemplate;
    }

    public List<User> getUsers(String text) {
        String url = baseUrl + "/admin/users";
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

            List<User> users = null;
            Map<String, Object> responseBody = apiResponse.getBody();
            if (responseBody != null && responseBody.containsKey("users"))
                users = Converter.convertToUsers((List<Map<String, Object>>) responseBody.get("users"));

            return users;
        } catch (HttpClientErrorException e) {
            return null;
        }
    }

    public Object createUser(String email) {
        HttpHeaders headers = new HttpHeaders();
        headers.setContentType(MediaType.APPLICATION_JSON);

        Map<String, String> body = new HashMap<>();
        body.put("email", email);

        HttpEntity<?> requestEntity = new HttpEntity<>(body, headers);

        try {
            ResponseEntity<Map<String, Object>> apiResponse = restTemplate.exchange(
                    baseUrl + "/admin/users/create",
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

    public Object updateUser(String id, String role, String status) {
        HttpHeaders headers = new HttpHeaders();
        headers.setContentType(MediaType.APPLICATION_JSON);

        Map<String, String> body = new HashMap<>();
        body.put("role", role);
        body.put("status", status);

        HttpEntity<?> requestEntity = new HttpEntity<>(body, headers);

        try {
            ResponseEntity<Map<String, Object>> apiResponse = restTemplate.exchange(
                    baseUrl + "/admin/users/update/" + id,
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

    public Object deleteUser(String id) {
        HttpHeaders headers = new HttpHeaders();
        headers.setContentType(MediaType.APPLICATION_JSON);

        HttpEntity<Map<String, String>> requestEntity = new HttpEntity<>(headers);
        try {
            ResponseEntity<Map<String, Object>> apiResponse = restTemplate.exchange(
                    baseUrl + "/admin/users/delete/" + id,
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

    public Object resetPassword(String id) {
        HttpHeaders headers = new HttpHeaders();
        headers.setContentType(MediaType.APPLICATION_JSON);

        HttpEntity<Map<String, String>> requestEntity = new HttpEntity<>(headers);
        try {
            ResponseEntity<Map<String, Object>> apiResponse = restTemplate.exchange(
                    baseUrl + "/admin/users/reset-password/" + id,
                    HttpMethod.GET,
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

    public Object changeAvatar(String id, String avatarUrl) {
        HttpHeaders headers = new HttpHeaders();
        headers.setContentType(MediaType.APPLICATION_JSON);

        Map<String, String> body = new HashMap<>();
        body.put("avatarUrl", avatarUrl);
        HttpEntity<?> requestEntity = new HttpEntity<>(body, headers);

        try {
            ResponseEntity<Map<String, Object>> apiResponse = restTemplate.exchange(
                    baseUrl + "/users/change-avatar/" + id,
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

    public Object changePassword(String id, String newPassword, String confirmPassword) {
        HttpHeaders headers = new HttpHeaders();
        headers.setContentType(MediaType.APPLICATION_JSON);

        Map<String, String> body = new HashMap<>();
        body.put("newPassword", newPassword);
        body.put("confirmPassword", confirmPassword);
        HttpEntity<?> requestEntity = new HttpEntity<>(body, headers);

        try {
            ResponseEntity<Map<String, Object>> apiResponse = restTemplate.exchange(
                    baseUrl + "/users/change-password/" + id,
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

        if (responseBody.containsKey("user"))
            return Converter.convertToUser((Map<String, Object>) responseBody.get("user"));
        else if (responseBody.containsKey("message"))
            return responseBody.get("message");
        else
            throw new HttpServerErrorException(HttpStatus.INTERNAL_SERVER_ERROR, "Empty response body");
    }
}
