package com.StoreManagementClient.Middlewares;

import com.StoreManagementClient.Models.User;
import jakarta.servlet.http.Cookie;
import jakarta.servlet.http.HttpServletRequest;
import jakarta.servlet.http.HttpServletResponse;
import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.beans.factory.annotation.Value;
import org.springframework.core.ParameterizedTypeReference;
import org.springframework.http.*;
import org.springframework.stereotype.Service;
import org.springframework.web.client.HttpClientErrorException;
import org.springframework.web.client.RestTemplate;
import org.springframework.web.server.ResponseStatusException;
import org.springframework.web.servlet.HandlerInterceptor;

import java.util.HashMap;
import java.util.Map;

@Service
public class AuthenticationInterceptor implements HandlerInterceptor {

    private final String baseUrl;
    private final RestTemplate restTemplate;

    @Autowired
    public AuthenticationInterceptor(@Value("${api.base.url}") String baseUrl, RestTemplate restTemplate) {
        this.baseUrl = baseUrl;
        this.restTemplate = restTemplate;
    }

    @Override
    public boolean preHandle(HttpServletRequest request, HttpServletResponse response, Object handler) throws Exception {
        String requestURI = request.getRequestURI();
        if (requestURI.equals("/error") ||
                requestURI.contains("js") ||
                requestURI.contains("css") ||
                requestURI.contains("img") ||
                requestURI.contains("vendors") ||
                requestURI.contains("reset-password"))
            return true;

        User user = isAuthenticated(request, response);
        if (user == null) {
            if (requestURI.equals("/auth/login")) return true;

            String contextPath = request.getContextPath();
            response.sendRedirect(contextPath + "/auth/login");
            return false;
        }

        request.setAttribute("authenticatedUser", user);
        if (requestURI.isEmpty() || requestURI.equals("/") || requestURI.equals("/home"))
            response.sendRedirect("/Home");
        return true;
    }

    public User isAuthenticated(HttpServletRequest request, HttpServletResponse response) {
        Cookie[] cookies = request.getCookies();
        if (cookies == null) return null;

        String token = null;
        for (Cookie cookie : cookies) {
            if (cookie.getName().equals("token")) {
                token = cookie.getValue();
                break;
            }
        }

        if (token == null || token.isEmpty()) return null;
        HttpHeaders headers = new HttpHeaders();
        headers.set("Authorization", "Bearer " + token);

        Map<String, Object> body = new HashMap<>();
        if (isOwnerOperations(request)) body.put("resource", "OWNER");
        else if (request.getRequestURI().contains("admin")) body.put("resource", "ADMIN");
        else body.put("resource", "EMPLOYEE");

        HttpEntity<?> requestEntity = new HttpEntity<>(body, headers);
        try {
            ResponseEntity<Map<String, Object>> apiResponse = restTemplate.exchange(
                    baseUrl + "/auth/validate",
                    HttpMethod.POST,
                    requestEntity,
                    new ParameterizedTypeReference<Map<String, Object>>() {
                    }
            );

            User user = null;
            Map<String, Object> responseBody = apiResponse.getBody();

            if (responseBody == null || !responseBody.containsKey("user")) {
                Cookie cookie = new Cookie("token", "");
                cookie.setMaxAge(0);
                cookie.setPath("/");
                response.addCookie(cookie);
            } else {
                user = Converter.convertToUser((Map<String, Object>) responseBody.get("user"));
            }

            return user;
        } catch (HttpClientErrorException e) {
            if (e.getStatusCode() == HttpStatus.FORBIDDEN)
                throw new ResponseStatusException(HttpStatus.FORBIDDEN, "You don't have permission to access this resource");

            return null;
        }
    }

    private boolean isOwnerOperations(HttpServletRequest request) {
        String requestURI = request.getRequestURI();
        if (requestURI.contains("admin")) {
            if (requestURI.contains("update")) {
                String role = request.getParameter("role");
                String oldRole = request.getParameter("oldRole");
                if (role != null && !role.equals(oldRole)) return true;
            }
        }

        return requestURI.contains("owner");
    }
}
