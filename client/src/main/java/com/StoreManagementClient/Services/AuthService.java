package com.StoreManagementClient.Services;

import com.StoreManagementClient.Middlewares.Converter;
import jakarta.servlet.http.Cookie;
import jakarta.servlet.http.HttpServletResponse;
import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.beans.factory.annotation.Value;
import org.springframework.core.ParameterizedTypeReference;
import org.springframework.http.*;
import org.springframework.stereotype.Service;
import org.springframework.web.client.HttpClientErrorException;
import org.springframework.web.client.HttpServerErrorException;
import org.springframework.web.client.RestTemplate;
import org.springframework.web.server.ResponseStatusException;

import java.util.HashMap;
import java.util.Map;

@Service
public class AuthService {
    private final String baseUrl;
    private final RestTemplate restTemplate;

    @Autowired
    public AuthService(@Value("${api.base.url}") String baseUrl, RestTemplate restTemplate) {
        this.baseUrl = baseUrl + "/auth";
        this.restTemplate = restTemplate;
    }

    public Object login(String username, String password, HttpServletResponse response) {
        HttpHeaders headers = new HttpHeaders();
        headers.setContentType(MediaType.APPLICATION_JSON);

        Map<String, String> body = Map.of("username", username, "password", password);
        HttpEntity<Map<String, String>> requestEntity = new HttpEntity<>(body, headers);

        try {
            ResponseEntity<Map<String, Object>> apiResponse = restTemplate.exchange(
                    baseUrl + "/login",
                    HttpMethod.POST,
                    requestEntity,
                    new ParameterizedTypeReference<Map<String, Object>>() {
                    }
            );

            String token = apiResponse.getHeaders().getFirst(HttpHeaders.AUTHORIZATION);
            token = token.substring(7);
            Cookie tokenCookie = new Cookie("token", token);
            tokenCookie.setPath("/");
            response.addCookie(tokenCookie);

            return getObjectFromApiResponse(apiResponse);
        } catch (HttpClientErrorException e) {
            Map<String, Object> responseBody = e.getResponseBodyAs(Map.class);
            if (responseBody == null || !responseBody.containsKey("message"))
                throw new HttpServerErrorException(HttpStatus.INTERNAL_SERVER_ERROR, "Empty response body");

            return responseBody.get("message");
        }
    }

    public Object resetPassword(String token, String newPassword, String confirmPassword) {
        HttpHeaders headers = new HttpHeaders();
        headers.setContentType(MediaType.APPLICATION_JSON);

        Map<String, String> body = new HashMap<>();
        body.put("newPassword", newPassword);
        body.put("confirmPassword", confirmPassword);
        HttpEntity<?> requestEntity = new HttpEntity<>(body, headers);

        try {
            ResponseEntity<Map<String, Object>> apiResponse = restTemplate.exchange(
                    baseUrl + "/reset-password?token=" + token,
                    HttpMethod.POST,
                    requestEntity,
                    new ParameterizedTypeReference<Map<String, Object>>() {
                    }
            );

            return getObjectFromApiResponse(apiResponse);
        } catch (HttpClientErrorException e) {
            if (e.getStatusCode() == HttpStatus.NOT_FOUND)
                throw new ResponseStatusException(HttpStatus.NOT_FOUND, "Invalid token");

            Map<String, Object> responseBody = e.getResponseBodyAs(Map.class);
            if (responseBody == null || !responseBody.containsKey("message"))
                throw new ResponseStatusException(HttpStatus.INTERNAL_SERVER_ERROR, "Empty response body");

            return responseBody.get("message");
        }
    }

    public void logout(HttpServletResponse response) {
        Cookie tokenCookie = new Cookie("token", "");
        tokenCookie.setPath("/");
        tokenCookie.setMaxAge(0);
        response.addCookie(tokenCookie);
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
