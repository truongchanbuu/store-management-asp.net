package com.StoreManagementClient.Controllers;

import com.StoreManagementClient.Models.Customer;
import com.StoreManagementClient.Models.Order;
import com.StoreManagementClient.Models.Product;
import com.StoreManagementClient.Models.User;
import com.StoreManagementClient.Services.CustomerService;
import com.StoreManagementClient.Services.OrderService;
import com.StoreManagementClient.Services.ProductService;
import jakarta.servlet.http.HttpServletRequest;
import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.stereotype.Controller;
import org.springframework.ui.Model;
import org.springframework.web.bind.annotation.*;
import org.springframework.web.servlet.mvc.support.RedirectAttributes;

import java.util.List;

@Controller
@RequestMapping("/customers")
public class CustomerController {
    private final CustomerService customerService;
    private final OrderService orderService;
    private final ProductService productService;

    @Autowired
    public CustomerController(CustomerService customerService, OrderService orderService, ProductService productService) {
        this.orderService = orderService;
        this.productService = productService;
        this.customerService = customerService;
    }

    @GetMapping("")
    public String getAllCustomers(@RequestParam(required = false) String phone, @RequestParam(required = false) String name, Model model, HttpServletRequest request) {
        User user = (User) request.getAttribute("authenticatedUser");
        model.addAttribute("user", user);

        List<Customer> customers = customerService.getUsers(phone, name);
        model.addAttribute("customers", customers);
        return "Customers/customer";
    }

    @GetMapping("/{id}")
    public String getCustomerById(@PathVariable String id, Model model, HttpServletRequest request) {
        User user = (User) request.getAttribute("authenticatedUser");
        model.addAttribute("user", user);

        Customer customer = customerService.getUserById(id);
        model.addAttribute("customer", customer);

        List<Order> orders = orderService.getOrderByCustomerId(id);
        model.addAttribute("orders", orders);

        List<Product> products = productService.getProducts(null);
        model.addAttribute("products", products);

        return "Customers/customer-detail";
    }

    @PostMapping("/update/{id}")
    public String updateCustomer(@PathVariable String id,
                                 @ModelAttribute Customer customer,
                                 Model model, RedirectAttributes redirectAttrs) {
        Object response = customerService.updateCustomer(id, customer);

        if (response instanceof String)
            redirectAttrs.addFlashAttribute("error", response);
        else
            redirectAttrs.addFlashAttribute("success", "Update customer success");

        redirectAttrs.addFlashAttribute("customer", customer);
        redirectAttrs.addFlashAttribute("user", model.getAttribute("user"));

        return "redirect:/customers/" + id;
    }
}
