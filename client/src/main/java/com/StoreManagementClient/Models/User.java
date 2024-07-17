package com.StoreManagementClient.Models;

import lombok.*;

@Getter
@Setter
@NoArgsConstructor
@AllArgsConstructor
@ToString
public class User {
    private String id;
    private String email;
    private String username;
    private String password;
    private Status status;
    private Role role;
    private String avatar;
}
