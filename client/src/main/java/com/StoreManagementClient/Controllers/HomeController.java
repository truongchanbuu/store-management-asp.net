package com.StoreManagementClient.Controllers;

import com.StoreManagementClient.Middlewares.AuthenticationInterceptor;
import com.StoreManagementClient.Models.User;
import com.StoreManagementClient.Services.AuthService;
import jakarta.servlet.RequestDispatcher;
import jakarta.servlet.http.HttpServletRequest;
import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.boot.web.servlet.error.ErrorController;
import org.springframework.stereotype.Controller;
import org.springframework.ui.Model;
import org.springframework.web.bind.annotation.GetMapping;
import org.springframework.web.bind.annotation.ModelAttribute;
import org.springframework.web.bind.annotation.RequestMapping;

@Controller
@RequestMapping("")
public class HomeController implements ErrorController {

    private final AuthenticationInterceptor utils;

    @Autowired
    public HomeController(AuthService authService, AuthenticationInterceptor utils) {
        this.utils = utils;
    }

    @GetMapping(value = "Home")
    public String index(Model model, @ModelAttribute("user") User user, HttpServletRequest request) {
        if (user != null && user.getUsername() != null)
            model.addAttribute("user", user);
        else {
            user = (User) request.getAttribute("authenticatedUser");
            if (user != null) model.addAttribute("user", user);
            else return "redirect:/auth/login";
        }

        return "index";
    }

    @RequestMapping("/error")
    public String handleException(HttpServletRequest request) {
        Object status = request.getAttribute(RequestDispatcher.ERROR_STATUS_CODE);

        // For debugging
        System.out.println(request.getAttribute(RequestDispatcher.ERROR_STATUS_CODE));
        System.out.println(request.getAttribute(RequestDispatcher.ERROR_MESSAGE));

        if (status != null) {
            int statusCode = Integer.parseInt(status.toString());
            if (statusCode == 404) return "Error/404";
            else if (statusCode == 401 || statusCode == 403) return "Error/401";
        }
        return "Error/500";
    }
}
