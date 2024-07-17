package com.StoreManagementClient.Controllers;

import com.StoreManagementClient.Models.User;
import com.StoreManagementClient.Services.AuthService;
import jakarta.servlet.http.HttpServletRequest;
import jakarta.servlet.http.HttpServletResponse;
import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.http.HttpStatus;
import org.springframework.stereotype.Controller;
import org.springframework.ui.Model;
import org.springframework.web.bind.annotation.GetMapping;
import org.springframework.web.bind.annotation.PostMapping;
import org.springframework.web.bind.annotation.RequestMapping;
import org.springframework.web.bind.annotation.RequestParam;
import org.springframework.web.server.ResponseStatusException;
import org.springframework.web.servlet.mvc.support.RedirectAttributes;

@Controller
@RequestMapping("/auth")
public class AuthController {

    private final AuthService authService;

    @Autowired
    public AuthController(AuthService authService) {
        this.authService = authService;
    }

    @GetMapping("/login")
    public String login(HttpServletRequest request, RedirectAttributes redirectAttrs) {
        User user = (User) request.getAttribute("authenticatedUser");

        if (user != null) {
            redirectAttrs.addFlashAttribute("user", user);
            return "redirect:/Home";
        }

        return "Auth/login";
    }

    @GetMapping("/reset-password")
    public String resetPassword(@RequestParam(required = false) String token) {
        if (token == null || token.isEmpty())
            throw new ResponseStatusException(HttpStatus.NOT_FOUND, "Token not found");

        return "Auth/reset_password";
    }

    @PostMapping("/reset-password")
    public String resetPasswordPost(@RequestParam(required = false) String token,
                                    @RequestParam("newPassword") String password,
                                    @RequestParam("confirmPassword") String confirmPassword,
                                    Model model) {

        if (token == null || token.isEmpty())
            throw new ResponseStatusException(HttpStatus.NOT_FOUND, "Token not found");

        Object response = authService.resetPassword(token, password, confirmPassword);
        if (response instanceof String)
            model.addAttribute("error", response);
        else
            model.addAttribute("success", "Reset password successfully. Please login again!");

        return "Auth/reset_password";
    }

    @PostMapping("/login")
    public String loginPost(@RequestParam("username") String username,
                            @RequestParam("password") String password,
                            HttpServletResponse response,
                            Model model, RedirectAttributes redirectAttrs) {

        Object res = authService.login(username, password, response);
        if (res == null) {
            model.addAttribute("error", "Invalid credentials!");
            return "Auth/login";
        } else if (res instanceof String) {
            model.addAttribute("error", res);
            return "Auth/login";
        }

        redirectAttrs.addFlashAttribute("user", res);
        return "redirect:/Home";
    }

    @GetMapping("/logout")
    public String logout(HttpServletRequest request, HttpServletResponse response) {
        User user = (User) request.getAttribute("authenticatedUser");
        if (user != null)
            authService.logout(response);

        return "redirect:/auth/login";
    }

}
