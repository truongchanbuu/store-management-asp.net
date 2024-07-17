package com.StoreManagementClient.Controllers;

import org.springframework.stereotype.Controller;
import org.springframework.ui.Model;
import org.springframework.web.bind.annotation.GetMapping;
import org.springframework.web.bind.annotation.ModelAttribute;
import org.springframework.web.bind.annotation.RequestMapping;

import com.StoreManagementClient.Models.User;

import jakarta.servlet.http.HttpServletRequest;

@Controller
@RequestMapping("/reports")
public class ReportController {
	@GetMapping
	public String index(Model model, @ModelAttribute("user") User user, HttpServletRequest request) {
		if (user != null && user.getUsername() != null)
            model.addAttribute("user", user);
        else {
            user = (User) request.getAttribute("authenticatedUser");
            if (user != null) model.addAttribute("user", user);
            else return "redirect:/auth/login";
        }
		
		return "Report/index";
	}
}
