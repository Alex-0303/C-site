namespace GraphQLTemplate.Types
{
    using System;
    using GraphQLTemplate.Constants;
    using GraphQLTemplate.DataLoaders;
    using GraphQLTemplate.Models;
    using GraphQLTemplate.Repositories;
    using HotChocolate.Resolvers;
    using HotChocolate.Types;

    public class HumanObject : ObjectType<Human>
    {
        private readonly IHumanRepository humanRepository;

        public HumanObject(IHumanRepository humanRepository) => this.humanRepository = humanRepository;

        protected override void Configure(IObjectTypeDescriptor<Human> descriptor)
        {
            if (descriptor is null)
            {
                throw new ArgumentNullException(nameof(descriptor));
            }

            descriptor
                .Name("Human")
                .Description("A humanoid creature from the Star Wars universe.")
                .Implements<CharacterInterface>();

#if Authorization
            // descriptor.Authorize(AuthorizationPolicyName.Admin); // To require authorization for all fields in this type.
#endif
            descriptor
                .ImplementsNode()
                .IdField(x => x.Id)
                .ResolveNode((context, id) => context.DataLoader<IHumanDataLoader>().LoadAsync(id, context.RequestAborted));
            // Replace with external resolver like so
            // .ResolveNodeWith<NodeResolver>(t => t.GetNodeAsync(default, default));
            descriptor
                .Field(x => x.Name)
                .Type<StringType>()
                .Description("The name of the human.");
            descriptor
                .Field(x => x.DateOfBirth)
                .Type<NonNullType<DateType>>()
#if Authorization
                .Authorize(AuthorizationPolicyName.Admin) // Require authorization to access the date of birth field.
#endif
                .Description("The humans date of birth.");
            descriptor
                .Field(x => x.HomePlanet)
                .Type<StringType>()
                .Description("The home planet of the human.");
            descriptor
                .Field(x => x.AppearsIn)
                .Type<ListType<NonNullType<EpisodeEnumeration>>>()
                .Description("Which movie they appear in.");
            descriptor
                .Field(x => x.Friends)
                .Type<ListType<NonNullType<CharacterInterface>>>()
                .Description("The friends of the character, or an empty list if they have none.")
                .Resolver(context => this.humanRepository.GetFriendsAsync(context.Parent<Human>(), context.RequestAborted));
        }
    }
}
